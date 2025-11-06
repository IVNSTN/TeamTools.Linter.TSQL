CREATE TABLE reconc.data_type
(
    code                  VARCHAR(30) NOT NULL
    , income_type         INT         NULL
    , need_parent_acc_int CHAR(1)     NULL
    , need_parent_acc_ext CHAR(1)     NULL
    , CONSTRAINT PK_reconc_data_type PRIMARY KEY CLUSTERED (code)
    , CONSTRAINT CK_reconc_data_type_need_parent_acc_int CHECK (need_parent_acc_int = 'Y' OR need_parent_acc_int = 'N')
    , CONSTRAINT CK_reconc_data_type_need_parent_acc_ext CHECK (need_parent_acc_ext = 'Y' OR need_parent_acc_ext = 'N')
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IDX_reconc_data_type_income_type
    ON reconc.data_type (income_type)
    INCLUDE (need_parent_acc_ext, need_parent_acc_int)
    WHERE income_type IS NOT NULL;
GO
