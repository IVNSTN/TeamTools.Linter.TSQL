WAITFOR (
    RECEIVE TOP(1) dummy
    from myqueue);
