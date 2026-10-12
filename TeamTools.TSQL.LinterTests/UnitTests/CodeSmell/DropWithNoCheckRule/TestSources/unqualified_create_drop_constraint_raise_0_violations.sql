-- Registers local constraint creation for an unqualified table.
-- The drop side defaults the schema to "dbo." while the create side (bug) keeps "C:DF_id",
-- causing a mismatch. Expected to raise 0 violations once the rule is correct.
CREATE TABLE t (id INT CONSTRAINT DF_id DEFAULT 0);

ALTER TABLE t DROP CONSTRAINT DF_id;
GO