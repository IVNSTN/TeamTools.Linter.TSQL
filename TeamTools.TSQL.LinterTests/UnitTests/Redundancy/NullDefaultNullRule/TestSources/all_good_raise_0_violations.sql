create table test.test
(
    id int not null identity(1,1),
    val int null,
    summary as (1 + 2),
    dt DATETIME DEFAULT GETDATE());

alter table test.test add constraint df_dt
default ((('null'))) for val;
