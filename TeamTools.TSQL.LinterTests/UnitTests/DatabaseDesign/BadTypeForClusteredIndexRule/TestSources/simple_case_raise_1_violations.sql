-- to prevent dup reports on single violation
CREATE TABLE Positions.SessionConversations
(
    SPID                    INT              NOT NULL
    , conversation_group_id UNIQUEIDENTIFIER NOT NULL
    , CONSTRAINT PK PRIMARY KEY CLUSTERED
      (conversation_group_id ASC, SPID ASC)
);
GO
