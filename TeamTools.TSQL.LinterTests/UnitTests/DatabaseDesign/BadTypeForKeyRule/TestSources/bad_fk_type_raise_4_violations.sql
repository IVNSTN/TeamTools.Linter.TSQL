CREATE TABLE dbo.tbl
(
    id          VARCHAR(MAX) PRIMARY KEY -- 1
    , type_id   FLOAT FOREIGN KEY REFERENCES dbo.record_types(type_id) -- 2
    , num       DECIMAL(18, 8)
    , dt        DATETIME
    , CONSTRAINT FK_num FOREIGN KEY (num, dt) REFERENCES dbo.nums(num, dt) -- 3
)

ALTER TABLE dbo.rbl ADD CONSTRAINT FK_DT FOREIGN KEY (dt) REFERENCES dbo.calendar(dt) -- 4
