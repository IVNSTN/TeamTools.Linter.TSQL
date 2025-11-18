using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0945", "REDUNDANT_GRANT")]
    [SecurityRule]
    internal sealed class RedundantGrantRule : AbstractRule
    {
        private static readonly string ObjectTypeDelim = "::";
        private static readonly string ErrTemplateSchema = Strings.ViolationDetails_RedundantGrantRule_AlreadyGrantedToSchema;
        private static readonly string ErrTemplateDup = Strings.ViolationDetails_RedundantGrantRule_AlreadyGranted;

        public RedundantGrantRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            // granted object / grantee / permission / permission node
            var permissions = new Dictionary<string, Dictionary<string, Dictionary<string, TSqlFragment>>>(StringComparer.OrdinalIgnoreCase);

            int n = node.Batches.Count;
            for (int i = 0; i < n; i++)
            {
                var batch = node.Batches[i];
                int m = batch.Statements.Count;

                for (int j = 0; j < m; j++)
                {
                    var stmt = batch.Statements[j];

                    if (stmt is GrantStatement grant)
                    {
                        ExtractPermissions(permissions, grant, true);
                    }
                    else if (stmt is DenyStatement deny)
                    {
                        ExtractPermissions(permissions, deny, false);
                    }
                }
            }

            ValidatePermissions(permissions);
        }

        private static string GrantPrefix(bool isGrant) => isGrant ? "GRANT-" : "DENY-";

        // TODO : total refactoring and simplification needed
        private void ExtractPermissions(
            Dictionary<string, Dictionary<string, Dictionary<string, TSqlFragment>>> permissions,
            SecurityStatement stmt,
            bool isGrant)
        {
            string target;

            if (stmt.SecurityTargetObject is null)
            {
                // e.g. VIEW SERVER STATE
                target = "SERVER-LEVEL";
            }
            else
            {
                target = (stmt.SecurityTargetObject.ObjectKind == SecurityObjectKind.NotSpecified ? "OBJECT" : stmt.SecurityTargetObject.ObjectKind.ToString())
                    + ObjectTypeDelim
                    + stmt.SecurityTargetObject.ObjectName.MultiPartIdentifier.Identifiers.GetFullName(TSqlDomainAttributes.NamePartSeparator);
            }

            if (!permissions.TryGetValue(target, out var permGrantees))
            {
                permGrantees = new Dictionary<string, Dictionary<string, TSqlFragment>>(StringComparer.OrdinalIgnoreCase);
                permissions.Add(target, permGrantees);
            }

            int n = stmt.Principals.Count;
            for (int i = 0; i < n; i++)
            {
                var grantee = stmt.Principals[i];
                string granteeName = grantee.PrincipalType.ToString();
                if (!(grantee.Identifier is null))
                {
                    // TODO : reduce string manufacturing
                    granteeName += ObjectTypeDelim + grantee.Identifier.Value;
                }

                if (!permGrantees.TryGetValue(granteeName, out var grantedPermissions))
                {
                    grantedPermissions = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);
                    permGrantees.Add(granteeName, grantedPermissions);
                }

                int m = stmt.Permissions.Count;
                for (int j = 0; j < m; j++)
                {
                    var perm = stmt.Permissions[j];
                    string permissionName = GrantPrefix(isGrant)
                        + perm.Identifiers.GetFullName(",");

                    if (!grantedPermissions.TryAdd(permissionName, perm))
                    {
                        // TODO : only extraction was supposed to happen here
                        HandleNodeError(perm, string.Format(ErrTemplateDup, permissionName));
                    }
                }
            }
        }

        // TODO : refactoring needed
        private void ValidatePermissions(Dictionary<string, Dictionary<string, Dictionary<string, TSqlFragment>>> permissions)
        {
            foreach (var schema in permissions)
            {
                if (schema.Key.StartsWith("SCHEMA::", StringComparison.OrdinalIgnoreCase))
                {
                    string schemaName = schema.Key.Substring(schema.Key.IndexOf(ObjectTypeDelim) + 2);
                    string objectPrefix = string.Format("OBJECT::{0}.", schemaName);

                    foreach (var schemaObject in permissions)
                    {
                        if (schemaObject.Key.StartsWith(objectPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            var sameGrantees = schema.Value.Keys.Intersect(schemaObject.Value.Keys, StringComparer.OrdinalIgnoreCase);
                            foreach (var grantee in sameGrantees)
                            {
                                var redundantPermission = schema.Value[grantee].Keys.Intersect(schemaObject.Value[grantee].Keys, StringComparer.OrdinalIgnoreCase).FirstOrDefault();

                                if (!(redundantPermission is null))
                                {
                                    HandleNodeError(
                                    schemaObject.Value[grantee][redundantPermission],
                                    string.Format(ErrTemplateSchema, redundantPermission, schemaName));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
