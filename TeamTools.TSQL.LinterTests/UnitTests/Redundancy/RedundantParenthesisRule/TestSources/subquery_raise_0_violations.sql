SELECT JSON_QUERY((
                      SELECT NULLIF(CONCAT('["', STRING_AGG(asset_synonym, '", "'), '"]'), '[""]')
                      FROM snm.actives_synonyms AS acts
                  )
                 ) AS asset_synonyms;
