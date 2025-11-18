exec my_proc;

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

SELECT n.f.value('.', 'varchar(100)')
FROM @xml.nodes('//Features/Feature') n (f)
