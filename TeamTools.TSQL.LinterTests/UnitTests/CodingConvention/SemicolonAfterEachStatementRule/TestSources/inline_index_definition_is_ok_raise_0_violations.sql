-- compatibility level min: 130
CREATE TABLE training.answers_detailed
(
    answer_list_id     BIGINT           NOT NULL IDENTITY(1, 1)
    , question_id      SMALLINT         NOT NULL
    , CONSTRAINT PK_training_answers_detailed PRIMARY KEY CLUSTERED (answer_list_id)
    , INDEX IDX_training_answers_detailed_session_id_is_last_question NONCLUSTERED (session_id, is_last_question)
    , INDEX IDX_training_answers_detailed_db_date_session_id NONCLUSTERED (db_date, session_id));
