CREATE PROCEDURE dbo.foo
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #result
    (
        rec_id                BIGINT       NOT NULL IDENTITY(1, 1) -- tested column
        , tran_no             BIGINT       NOT NULL
        , reg_code            VARCHAR(30)  NOT NULL
        , direction           CHAR(1)      NOT NULL
        , depo_account        VARCHAR(21)  NOT NULL
        , extra_depo_account  VARCHAR(21)  NULL
        , ammount             BIGINT       NOT NULL
        , profit_centr        VARCHAR(20)  NOT NULL
        , db_date             DATETIME     NOT NULL
        , depo_code           VARCHAR(20)  NOT NULL
        , acc_code            VARCHAR(20)  NULL
        , p_code              VARCHAR(20)  NOT NULL
        , settle_comments     VARCHAR(255) NULL
        , acc_code2           VARCHAR(20)  NULL
        , depo_account2       VARCHAR(21)  NULL
        , extra_depo_account2 VARCHAR(21)  NULL
        , isin                VARCHAR(20)  NULL
        , short_name          VARCHAR(100) NULL
        , place_code          VARCHAR(100) NULL
        , ver                 BINARY(8)    NULL
        , PRIMARY KEY (rec_id)
    );

    SET IDENTITY_INSERT #result ON; -- here rec_id becomes required 

    INSERT #result
    (
        rec_id -- here it is explicitly filled
        , tran_no
        , reg_code
        , direction
        , depo_account
        , extra_depo_account
        , ammount
        , profit_centr
        , db_date
        , depo_code
        , acc_code
        , p_code
        , settle_comments
        , acc_code2
        , depo_account2
        , extra_depo_account2
        , isin
        , short_name
        , place_code
        , ver
    )
    SELECT
        ROW_NUMBER() OVER (ORDER BY direction, p_code, depo_account, ammount) AS rec_id
        , tran_no
        , reg_code
        , direction
        , depo_account
        , extra_depo_account
        , ammount
        , profit_centr
        , db_date
        , depo_code
        , acc_code
        , p_code
        , settle_comments
        , acc_code2
        , depo_account2
        , extra_depo_account2
        , isin
        , short_name
        , place_code
        , ver
    FROM dbo.v_aex_deponet_export# AS de;

    SET IDENTITY_INSERT #result OFF; -- here rec_id should become not required

    INSERT #result
    (
        tran_no -- here rec_id is omitted and it should be fine
        , reg_code
        , direction
        , depo_account
        , extra_depo_account
        , ammount
        , profit_centr
        , db_date
        , depo_code
        , acc_code
        , p_code
        , settle_comments
        , acc_code2
        , depo_account2
        , extra_depo_account2
        , isin
        , short_name
        , place_code
        , ver
    )
    SELECT
        tran_no
        , reg_code
        , direction
        , depo_account
        , extra_depo_account
        , ammount
        , profit_centr
        , db_date
        , depo_code
        , acc_code
        , p_code
        , settle_comments
        , acc_code2
        , depo_account2
        , extra_depo_account2
        , isin
        , short_name
        , place_code
        , ver
    FROM #archive
    WHERE direction <> 'D';
END;
GO
