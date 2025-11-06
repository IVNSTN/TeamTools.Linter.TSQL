using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0945", "REDUNDANT_GRANT")]
    [SecurityRule]
    internal sealed class RedundantGrantRule : AbstractRule
    {
        private static readonly string ObjectTypeDelim = "::";
        private static readonly string ErrTemplateSchema = "{0} already given on schema {1}";
        private static readonly string ErrTemplateDup = "{0} already granted";

        public RedundantGrantRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            // granted object / grantee / permission / permission node
            var permissions = new Dictionary<string, IDictionary<string, IDictionary<string, TSqlFragment>>>(StringComparer.OrdinalIgnoreCase);

            foreach (var batch in node.Batches)
            {
                foreach (var stmt in batch.Statements)
                {
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

        // TODO : total refactoring and simplification needed
        private void ExtractPermissions(
            Dictionary<string, IDictionary<string, IDictionary<string, TSqlFragment>>> permissions,
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
                    + string.Join(
                        TSqlDomainAttributes.NamePartSeparator,
                        stmt.SecurityTargetObject.ObjectName.MultiPartIdentifier.Identifiers.Select(id => id.Value));
            }

            if (!permissions.ContainsKey(target))
            {
                permissions.Add(target, new Dictionary<string, IDictionary<string, TSqlFragment>>(StringComparer.OrdinalIgnoreCase));
            }

            foreach (var grantee in stmt.Principals)
            {
                string granteeName = grantee.PrincipalType.ToString();
                if (!(grantee.Identifier is null))
                {
                    granteeName += ObjectTypeDelim + grantee.Identifier.Value;
                }

                if (!permissions[target].ContainsKey(granteeName))
                {
                    permissions[target].Add(granteeName, new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase));
                }

                foreach (var perm in stmt.Permissions)
                {
                    string permissionName = (isGrant ? "GRANT-" : "DENY-")
                        + string.Join(",", perm.Identifiers.Select(id => id.Value));

                    if (permissions[target][granteeName].ContainsKey(permissionName))
                    {
                        // TODO : only extraction was supposed to happen here
                        HandleNodeError(perm, string.Format(ErrTemplateDup, permissionName));
                    }
                    else
                    {
                        permissions[target][granteeName].Add(permissionName, perm);
                    }
                }
            }
        }

        private void ValidatePermissions(Dictionary<string, IDictionary<string, IDictionary<string, TSqlFragment>>> permissions)
        {
            foreach (var schema in permissions.Keys.Where(key => key.StartsWith("SCHEMA::", StringComparison.OrdinalIgnoreCase)))
            {
                string schemaName = schema.Substring(schema.IndexOf(ObjectTypeDelim) + 2);
                string objectPrefix = string.Format("OBJECT::{0}.", schemaName);

                foreach (var schemaObject in permissions.Keys.Where(key => key.StartsWith(objectPrefix, StringComparison.OrdinalIgnoreCase)))
                {
                    var sameGrantees = permissions[schema].Keys.Intersect(permissions[schemaObject].Keys, StringComparer.OrdinalIgnoreCase);
                    foreach (var grantee in sameGrantees)
                    {
                        var redundantPermission = permissions[schema][grantee].Keys.Intersect(permissions[schemaObject][grantee].Keys, StringComparer.OrdinalIgnoreCase).FirstOrDefault();

                        if (!(redundantPermission is null))
                        {
                            HandleNodeError(
                            permissions[schemaObject][grantee][redundantPermission],
                            string.Format(ErrTemplateSchema, redundantPermission, schemaName));
                        }
                    }
                }
            }
        }
    }
}
