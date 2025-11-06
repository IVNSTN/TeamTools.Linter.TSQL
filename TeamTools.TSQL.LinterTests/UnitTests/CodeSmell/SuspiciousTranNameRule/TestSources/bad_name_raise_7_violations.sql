-- here THROW is treated as a separate command but pretends to be tran name
BEGIN TRAN THROW

-- here THROW is treated as tran name, not command!
SAVE TRAN THROW

-- here THROW is treated as a separate command but pretends to be tran name
COMMIT TRAN THROW

-- here THROW is treated as tran name, not command!
ROLLBACK TRAN THROW

GO

-- here RETURN is treated as a separate command but pretends to be tran name
BEGIN TRAN RETURN

-- here RETURN is treated as invalid syntax
-- SAVE TRAN RETURN

-- here RETURN is treated as a separate command but pretends to be tran name
COMMIT TRAN RETURN

-- here RETURN is treated as a separate command but pretends to be tran name
ROLLBACK TRAN RETURN
GO
