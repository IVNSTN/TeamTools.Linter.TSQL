WAITFOR (GET CONVERSATION GROUP @ConversationGroupId FROM backend._TargetQueue_MainEntities),
TIMEOUT 100;

WAITFOR (GET CONVERSATION GROUP @ConversationGroupId FROM backend._TargetQueue_MainEntities),
TIMEOUT @timeout;
