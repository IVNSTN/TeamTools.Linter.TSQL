CREATE TABLE #oper_transfers_src
(
    trn_no           INT            NOT NULL
    , oper_id        INT            NOT NULL
    , cur_group      DECIMAL(18, 8) NULL PRIMARY KEY (oper_id, trn_no)
);
