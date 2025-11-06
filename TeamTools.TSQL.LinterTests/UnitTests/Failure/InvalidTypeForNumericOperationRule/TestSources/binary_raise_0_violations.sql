DECLARE @bin VARBINARY(100) = 127


SET @bin |= @bin & 0x01
