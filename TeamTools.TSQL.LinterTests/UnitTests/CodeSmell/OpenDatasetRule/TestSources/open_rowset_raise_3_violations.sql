SELECT *
FROM OPENDATASOURCE('SQLNCLI',
    'Data Source=London\Payroll;Integrated Security=SSPI')
    .AdventureWorks2022.HumanResources.Employee;

SELECT a.*
FROM OPENROWSET(
    'SQLNCLI',
    'Server=Seattle1;Trusted_Connection=yes;',
    'SELECT TOP 10 GroupName, Name FROM AdventureWorks2022.HumanResources.Department'
) AS a;

SELECT * FROM OPENROWSET(
    BULK N'C:\Text1.txt',
    SINGLE_NCLOB
) AS Document;
