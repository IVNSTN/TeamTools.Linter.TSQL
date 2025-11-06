DECLARE
    @NewInstruments    CURSOR
    , @p_code          VARCHAR(20)
    , @ModeMarketBoard CHAR(1)
    , @IdObject        INT;
