DECLARE @docs TABLE
(
    NumEDocument            BIGINT        NOT NULL
    , IdDocumentType        TINYINT       NOT NULL
    , IdCommandType         INT           NOT NULL
    , SignTime              DATETIME      NOT NULL
    , AcceptTime            DATETIME      NOT NULL
    , ClientCode            VARCHAR(50)   NOT NULL
    , Login                 VARCHAR(50)   NOT NULL
    , IdAccount             INT           NOT NULL
    , IdLogin               INT           NOT NULL
    , IdCertificate         BIGINT        NOT NULL
    , IdOrderType           INT           NOT NULL
    , IdOrderStatus         INT           NOT NULL
    , IdObject              INT           NOT NULL
    , IdLifeTime            INT           NOT NULL
    , IdExecutionType       INT           NOT NULL
    , ActivationTime        DATETIME      NOT NULL
    , Comment               VARCHAR(1024) NOT NULL
    , Contragent            VARCHAR(25)   NOT NULL
    , Reason                VARCHAR(255)  NOT NULL
    , SettleDay             INT           NOT NULL
    , EntryFilledQuantity   INT           NOT NULL
    , SessionFilledQuantity INT           NOT NULL
    , OrderUpdateTime       BIGINT        NOT NULL
    , OrderStatusRevision   INT           NOT NULL
    , Revision              BIGINT        NOT NULL
    , Life                  INT           NOT NULL
    , DtInsert              DATETIME2(7)  NOT NULL
    , DtUpdate              DATETIME2(7)  NOT NULL
    , IdSignSource          INT           NOT NULL
    , NumSDocument          BIGINT        NOT NULL
    , SignType              INT           NOT NULL
    , Flags                 BIGINT        NOT NULL
    , PRIMARY KEY CLUSTERED
      (
          NumEDocument
          , IdDocumentType
          , IdCommandType
          , SignTime
          , AcceptTime
          , ClientCode
          , Login
          , IdAccount
          , IdLogin
          , IdCertificate
          , IdOrderType
          , IdOrderStatus
          , IdObject
          , Contragent
          , Reason
          , SettleDay
          , EntryFilledQuantity
          , SessionFilledQuantity
          , OrderUpdateTime
          , OrderStatusRevision
          , Revision
          , Life
          , DtInsert
          , DtUpdate
          , IdSignSource
          , NumSDocument
          , SignType
          , Flags
      )
);
GO
