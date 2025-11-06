EXEC sp_OACreate '', @handle

EXEC sys.sp_add_job @job_id = @job_id

EXEC sp_xml_preparedocument @handle
