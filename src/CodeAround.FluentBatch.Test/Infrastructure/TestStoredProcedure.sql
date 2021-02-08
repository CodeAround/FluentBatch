IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GivePersons_By_personID]') AND type in (N'P', N'PC'))
BEGIN
DROP PROCEDURE [dbo].GivePersons_By_personID
end
EXEC('CREATE PROCEDURE [dbo].[GivePersons_By_personID] 
	@PersonId VARCHAR(50)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * FROM dbo.Persons WHERE PersonId = @PersonId
END')

