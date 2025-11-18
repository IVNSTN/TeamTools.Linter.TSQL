-- There is a separate rule to require explicit column nullability definition
DECLARE @foo TABLE
(
    id       INT PRIMARY KEY    -- db engine will make it not nullable
)
