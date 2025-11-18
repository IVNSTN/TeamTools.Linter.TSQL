EXEC tSQLt.ApplyTrigger
    @TableName = N'foo'
    , @TriggerName = '﻿dbo.bar';    --<< first char in this literal
