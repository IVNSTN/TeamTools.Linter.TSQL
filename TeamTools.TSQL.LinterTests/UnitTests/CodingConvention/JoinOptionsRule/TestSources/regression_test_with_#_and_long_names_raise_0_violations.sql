UPDATE AllowedOrderParamsByLogin
SET
    AllowedOrderParamsByLogin.operation = 'D'
    , AllowedOrderParamsByLogin.version = CASE
                                              WHEN AllowedOrderParamsByLogin.version > AllowedOrderParams.version THEN
                                                  AllowedOrderParamsByLogin.version
                                              ELSE
                                                  AllowedOrderParams.version
                                          END
FROM #AllowedOrderParamsByLogin AS AllowedOrderParamsByLogin
INNER JOIN #AllowedOrderParams AS AllowedOrderParams
    ON AllowedOrderParamsByLogin.IdAllowedOrderParams = AllowedOrderParams.IdAllowedOrderParams
WHERE AllowedOrderParams.operation = 'D'
    AND AllowedOrderParamsByLogin.operation <> 'D';
