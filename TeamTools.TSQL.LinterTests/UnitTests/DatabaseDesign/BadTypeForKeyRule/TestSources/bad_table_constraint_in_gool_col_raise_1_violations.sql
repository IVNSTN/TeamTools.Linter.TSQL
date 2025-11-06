CREATE TABLE #oper_transfers_src
(
    trn_no           INT            NOT NULL
    , cur_group      DECIMAL(18, 8) NULL 
    , oper_id        INT            NOT NULL PRIMARY KEY (cur_group)
);
