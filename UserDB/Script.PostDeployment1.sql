/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
/*  if were empty we fill some sample data for testing */
if not exists (select 1 from dbo.[User])
begin
    insert into dbo.[User] (FirstName, LastName)
    values('Tim', 'Hames'),('Bob', 'Ross'),('Amber','Spender'),('Cameron','Griffin');
end