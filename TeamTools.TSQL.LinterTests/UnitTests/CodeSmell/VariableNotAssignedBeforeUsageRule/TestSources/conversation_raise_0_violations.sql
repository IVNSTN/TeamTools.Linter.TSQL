DECLARE
    @ConversationGroupId UNIQUEIDENTIFIER
    , @DialogHandle      UNIQUEIDENTIFIER;

WAITFOR (GET CONVERSATION GROUP @ConversationGroupId FROM dbo.foo_queue);

SELECT @ConversationGroupId;

WAITFOR
(
    RECEIVE TOP (1) @DialogHandle = conversation_handle
        FROM dbo.foo_queue
        WHERE CONVERSATION_GROUP_ID = @ConversationGroupId
),
TIMEOUT 100;

SELECT @DialogHandle;
GO

DECLARE @tbl_var dbo.table_type;

WAITFOR (RECEIVE TOP (1) * FROM dbo.foo_queue
INTO @tbl_var
WHERE CONVERSATION_GROUP_ID = @ConversationGroupId);

SELECT * FROM @tbl_var
GO

DECLARE @chandle UNIQUEIDENTIFIER;

BEGIN DIALOG CONVERSATION @chandle
    FROM SERVICE [InitiatorService]
    TO SERVICE 'TargetService';

SELECT @chandle;
