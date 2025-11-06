SET @log_text += '; Входные параметры: '
                  + CONCAT(
                              '@doc_id: '
                              , @doc_id
                              , ', @is_ad:'
                              , @is_ad
                              , (
                                    SELECT
                                        (
                                            SELECT
                                                CONCAT(
                                                          ' eq_login_click:'
                                                          , eq_login_click
                                                      )
                                            FROM @simple_sign
                                            FOR XML PATH(''), TYPE
                                        ).value('.', 'nvarchar(max)')
                                ));
