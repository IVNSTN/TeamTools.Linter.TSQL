DECLARE
    @a NATIONAL CHARACTER VARYING (100) -- 1
    -- FIXME: bugreport https://github.com/microsoft/SqlScriptDOM/issues/116
    -- waiting for bug fix in ScriptDom - next string gets parsed as FLOAT...
    -- , @b DOUBLE PRECISION               -- 2
    , @b DEC                            -- 2
    , @c INTEGER                        -- 3
