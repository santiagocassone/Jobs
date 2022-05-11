  UPDATE [dbo].[LookupGeneral] SET Value = 'InstallDeliver' WHERE Name = 'LaborQuoteType' AND Value = 'Installation / Delivery'
  UPDATE [dbo].[LookupGeneral] SET Value = 'ProjMan' WHERE Name = 'LaborQuoteType' AND Value = 'Project Management'
  UPDATE [dbo].[LookupGeneral] SET Value = 'ShipReq' WHERE Name = 'LaborQuoteType' AND Value = 'Shipping Request'
  
    DELETE FROM [dbo].[LookupGeneral] WHERE Name = 'LaborQuoteLaborTypeValue'

  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'Reg', '49.00', 1, 1, GETDATE(), 'SYSTEM', 'W')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'OT', '98.00', 2, 1, GETDATE(), 'SYSTEM', 'W')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'DT', '73.50', 3, 1, GETDATE(), 'SYSTEM', 'W')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'PWR', '72.00', 4, 1, GETDATE(), 'SYSTEM', 'W')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'PWOT', '108.00', 5, 1, GETDATE(), 'SYSTEM', 'W')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'PWDT', '144.00', 6, 1, GETDATE(), 'SYSTEM', 'W')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'Lead', '40.20', 1, 1, GETDATE(), 'SYSTEM', 'S')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'Inst', '33.00', 2, 1, GETDATE(), 'SYSTEM', 'S')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'OT', '60.30', 3, 1, GETDATE(), 'SYSTEM', 'S')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'PWR', '65.00', 4, 1, GETDATE(), 'SYSTEM', 'S')
	  INSERT INTO [dbo].[LookupGeneral] (ObjectCustomConfigID, Name, Value, TranslateValue, ListOrder, IsActive, CreateTime, CreatedBy, Company_Code)
	VALUES (2, 'LaborQuoteLaborTypeValue', 'PWOT', '97.50', 5, 1, GETDATE(), 'SYSTEM', 'S')

	DELETE FROM [dbo].[LookupGeneral] WHERE Name = 'LaborQuoteLaborType' AND Company_Code = 'S' AND Value = 'PWDT'

	UPDATE [dbo].[LookupGeneral] SET Value = 'Lead', TranslateValue = 'Lead' WHERE Name = 'LaborQuoteLaborType' AND Company_Code = 'S' AND Value = 'Reg'
	UPDATE [dbo].[LookupGeneral] SET Value = 'Inst', TranslateValue = 'Installer' WHERE Name = 'LaborQuoteLaborType' AND Company_Code = 'S' AND Value = 'OT'
	UPDATE [dbo].[LookupGeneral] SET Value = 'OT', TranslateValue = 'OT' WHERE Name = 'LaborQuoteLaborType' AND Company_Code = 'S' AND Value = 'DT'
	UPDATE [dbo].[LookupGeneral] SET Value = 'PWR', TranslateValue = 'PW Reg' WHERE Name = 'LaborQuoteLaborType' AND Company_Code = 'S' AND Value = 'PWR'
	UPDATE [dbo].[LookupGeneral] SET Value = 'PWOT', TranslateValue = 'PW OT' WHERE Name = 'LaborQuoteLaborType' AND Company_Code = 'S' AND Value = 'PWOT'