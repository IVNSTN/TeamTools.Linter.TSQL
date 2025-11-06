declare @var int = @@TRANCOUNT,
    @dt DATETIME = GETDATE(),
    @int VARCHAR(10) = CAST(1 as VARCHAR(10)),
    @xml_value VARCHAR(10) = @xml.value('.', 'varchar(10)')
