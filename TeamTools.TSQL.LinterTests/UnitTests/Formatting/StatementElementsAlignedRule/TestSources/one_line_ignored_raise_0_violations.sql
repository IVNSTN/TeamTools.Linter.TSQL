UPDATE far set jar = par FROM far WHERE nar != zar;

DELETE far FROM far WHERE nar != zar;

MERGE x USING y ON x.a = y.b WHEN MATCHED THEN DELETE;