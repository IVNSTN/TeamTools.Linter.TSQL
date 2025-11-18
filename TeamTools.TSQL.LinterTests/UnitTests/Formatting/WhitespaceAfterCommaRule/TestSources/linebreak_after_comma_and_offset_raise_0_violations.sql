BEGIN
    -- see trailing comma
    WAITFOR (GET CONVERSATION GROUP @ConversationGroupId FROM foo.bar_queue),
    TIMEOUT 1000;

    INSERT INTO dbo.foo (some_name, state_value)
    VALUES
    ('Test', 'N'),
    ('Test', 'Y');
END
