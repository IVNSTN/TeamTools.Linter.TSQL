-- compatibility level min: 110
THROW 50000, 'asdf', 0;
THROW @errnum, 'asdf', 255;
THROW 999999, 'asdf', @state;
