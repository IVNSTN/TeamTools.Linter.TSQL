CREATE SERVICE [MainEntities_TargetService]
    AUTHORIZATION dbo
    ON QUEUE dbo._TargetQueue_MainEntities
    ([MainEntities_contract], [MainEntities_contract_a76t]);
GO

GRANT SEND
    ON SERVICE::[MainEntities_TargetService]
    TO PUBLIC;
GO
