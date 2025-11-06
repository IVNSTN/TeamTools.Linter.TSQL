WAITFOR
(
    RECEIVE TOP (1) @DialogHandle = conversation_handle
        FROM dbo.foo_queue
        WHERE CONVERSATION_GROUP_ID = @ConversationGroupId
),
TIMEOUT 100;
