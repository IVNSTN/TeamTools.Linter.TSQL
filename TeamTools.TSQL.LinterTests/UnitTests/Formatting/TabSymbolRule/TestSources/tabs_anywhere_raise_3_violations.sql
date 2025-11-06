/*	avoid saving in IDE which autoreplaces tabs with spaces!	*/
SELECT	'a',
'b'				--	comment
GO
	EXEC	my_proc
	INSERT	#tbl(a,	  b, 	 c)
	VALUES	(a,	b,	c)
GO	