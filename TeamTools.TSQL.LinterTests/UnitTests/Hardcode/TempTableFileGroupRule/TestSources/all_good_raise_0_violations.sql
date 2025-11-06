CREATE TABLE foo (id int ) ON [PRIMARY]

DECLARE @zar TABLE (id int)

CREATE INDEX IDX_fee_treaty_place_code
ON dbo.bar (treaty, place_code)
ON [PRIMARY];

CREATE UNIQUE INDEX #IDX_fee_treaty_place_code
ON #fee (treaty, place_code);
