EXEC sp_OACreate '', @handle OUTPUT

EXEC sys.sp_add_job @job_id = @job_id OUTPUT

EXEC sp_xml_preparedocument @handle OUTPUT
