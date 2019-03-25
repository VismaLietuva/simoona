CREATE FUNCTION dbo.ConvertXmlToJson(@xmlString xml)
RETURNS varchar(max)
BEGIN

RETURN (SELECT '['+Stuff(  
  (SELECT * from  
    (SELECT ',{'+  
      Stuff((SELECT ',"'+coalesce(b.c.value('local-name(.)', 'NVARCHAR(MAX)'),'')+'":"'+
                    b.c.value('text()[1]','NVARCHAR(MAX)') +'"'
             from x.a.nodes('*') b(c)  
             for xml path(''),TYPE).value('(./text())[1]','NVARCHAR(MAX)')
        ,1,1,'')+'}' 
   from @xmlString.nodes('/root/*') x(a)  
   ) JSON(theLine)  
  for xml path(''),TYPE).value('.','NVARCHAR(MAX)' )
,1,1,'')+']')

END;
GO
