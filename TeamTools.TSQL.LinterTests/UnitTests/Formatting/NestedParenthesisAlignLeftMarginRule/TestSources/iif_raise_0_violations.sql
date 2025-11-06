-- compatibility level min: 110
SELECT IIF (
                (select 1) <
                       (a + 
                        (d)
                       )
             , 0
             , 1
           )

            SET @error_msg =
IIF(ERROR_NUMBER() < 50000
    , CONCAT('Ошибка № ', ERROR_NUMBER(), ' в строке ', ERROR_LINE(), ' модуля: ', ERROR_PROCEDURE(), '. Текст ошибки: ', ERROR_MESSAGE())
    , ERROR_MESSAGE());
