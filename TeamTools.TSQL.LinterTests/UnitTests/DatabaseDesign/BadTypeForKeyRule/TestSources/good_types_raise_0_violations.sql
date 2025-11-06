CREATE TABLE dbo.tbl
(
    id          VARCHAR(256) PRIMARY KEY -- short string
    , type_id   SMALLINT FOREIGN KEY REFERENCES dbo.record_types(type_id)
    , num       BIGINT
    , dt        DATE
    , CONSTRAINT FK_num FOREIGN KEY (num, dt) REFERENCES dbo.nums(num, dt)
)

ALTER TABLE dbo.tbl ADD CONSTRAINT PK FOREIGN KEY (num) REFERENCES dbo.num (num)

ALTER TABLE dbo.tbl ADD CONSTRAINT PK PRIMARY KEY (dt)

CREATE TABLE dbo.bar (
    name SYSNAME PRIMARY KEY
)
