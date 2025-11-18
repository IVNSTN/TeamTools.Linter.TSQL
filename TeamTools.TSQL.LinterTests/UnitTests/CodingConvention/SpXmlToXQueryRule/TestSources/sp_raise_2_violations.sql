DECLARE @xml XML = N'
    <Product>
        <Name>Mouse</Name>
        <Category>Electronics</Category>
        <Price>25.00</Price>
        <Features>
            <Feature>Wireless</Feature>
            <Feature>Ergonomic</Feature>
        </Features>
    </Product>';
DECLARE @handle INT

EXEC sp_xml_preparedocument @handle OUT, @xml           -- 1

SELECT * FROM OPENXML(@handle, '//Features/Feature')    -- 2
WITH (Feature VARCHAR(100) '.')

EXEC sp_xml_removedocument @handle
