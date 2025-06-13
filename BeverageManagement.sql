  
  
-- =============================================        
-- Author:<Ananya Ganguly>        
-- Create date: <04/04/2024>        
-- Description:<This procedure will do Export for entire MS Active Data based on row limit>        
-- =============================================    
  
  
CREATE   PROCEDURE [dbo].[SP_List_MsActivity]                        
@request_payload nvarchar(max),          
@row_limit int          
          
AS          
BEGIN          
          
 SET NOCOUNT ON;                           
 -- flag for checking if list is getting called for treeview export            
  DECLARE @isTreeView bit =0;             
 -- for export paginated should be false.               
 DECLARE @paginated bit = 1;            
          
               
 -- size of the resultset                                
 DECLARE @pageSize int = 100;                                
 DECLARE @offset int = 0;                                
 DECLARE @fetch_size int = 100;                                
 DECLARE @isDistinct varchar(10) = 'false';                                
 -- page number for which data is required                                
 DECLARE @pageNumber int = 1;          
  DECLARE @countQuery nvarchar(max) = '';           
                                
 DECLARE @locale varchar = 'E';                                
 -- varibale to hold payload parameter                                
 DECLARE @jsonPayload nvarchar(max) = @request_payload;                                        
                                   
 DECLARE @userId varchar(50);                 
 DECLARE @view varchar(50)          
    ,@division varchar(5)           
   ,@lob varchar(5)          
   ,@product varchar(5)          
   ,@orgLevel varchar(15)           
   ,@orgCode varchar(10)          
   ,@year varchar(10)          
   ,@month varchar(10)           
          
 DECLARE @offset_fetch varchar(200) = '';           
  DECLARE @search_params Table(                              
       [Key] varchar(30),                              
       [Value] varchar(max)                    
    --,[Value2] varchar(max)                    
      );                    
             
  INSERT INTO  @search_params          
 Select Json_Value([value],'$.key') 'Key', Json_Value([value],'$.values[0]') 'Value'                              
 From OpenJson((Select Json_Query(@jsonPayload,'$.parameters')));             
 --select * from @search_params          
                    
  Select @userId = [Value] from @search_params where [Key] = 'userId';                   
  Select @view = [Value] from @search_params where [Key] = 'view';           
  Select @month= [Value] from @search_params where [Key] = 'month';                   
  Select @year = [Value] from @search_params where [Key] = 'year';           
  Select @division = [Value] from @search_params where [Key] = 'division';                   
  Select @lob = [Value] from @search_params where [Key] = 'lob';           
  Select @orgLevel = [Value] from @search_params where [Key] = 'orgLevel';                   
  Select @orgCode= [Value] from @search_params where [Key] = 'orgCode';           
  Select @product = [Value] from @search_params where [Key] = 'product';           
      
               
   IF @view = 'tree'            
   BEGIN            
  set @isTreeView = 1;            
   END            
 --select * From @search_params            
           
 DECLARE @generated_clause nvarchar(max) = '';                                
 DECLARE @sorting_sql nvarchar(max) = '';                                
 DECLARE @exportOption varchar(5)='CRT';                              
 DECLARE @isGroupByExists int = 0;                              
 DECLARE @selectColomJsonForGroupBy nvarchar(max) = '';                              
 DECLARE @groupBy_sql nvarchar(max) = '';                              
 DECLARE @distinct nvarchar(max) = '';                               
 DECLARE @page_info table (                                
  ExportOption varchar(20),       
  PageSize int,      
  PageNumber int,                                
  Locale varchar,                                
  Offset int,                                
  FetchSize int,         
  IsDistinct varchar(10)                                
 );          
        
        
                     
 -- we would require the actual column id to generate the               
 DECLARE @search_option table (                                
  CLAUSE nvarchar(max),                                
  FIELD_KEY nvarchar(max),                                
  TYPE nvarchar(max),                                
  OPERATOR nvarchar(max),                                
  VALUE nvarchar(max)                                
 )                           
                      
 -- Step 1: Get the paging information from the payload                                
 -- Step 2: Enforce row limiting logic                                
 INSERT INTO @page_info EXEC SP_Utility_GetFasatPagingInformation @request_payload=@jsonPayload,@row_limit = @row_limit;                                 
                                
 SELECT @pageSize = PageSize, @pageNumber = PageNumber, @locale = Locale, @offset = Offset, @fetch_size = FetchSize, @isDistinct = IsDistinct, @exportOption = ExportOption           
 FROM @page_info;                                
                                 
 --select @locale                               
                                 
 IF @pageSize < 0                                
 BEGIN                                
                                   
   RAISERROR('INVALID_PAGINATION',16,1);                                
   RETURN;                                
                                   
 END                                
 IF @fetch_size > 0                                
 BEGIN                                
  SET @offset_fetch = CONCAT(' OFFSET ',@offset,' ROWS FETCH NEXT ',@fetch_size, ' ROWS ONLY');                                
 END                                
 ELSE                                
 BEGIN                                
  SET @offset_fetch = ' ';                                
 END          
           
 ----To calculate Period          
 DECLARE @archiveYear varchar(max)          
 DECLARE @getArchieveDatabaseName varchar(max)          
 DECLARE @fasat_Archive_Database_Name varchar(max) ;          
 DECLARE @getArchiveYear varchar(max) ;          
 DECLARE @getArchiveCurrentYear varchar(max);         
 DECLARE @flagIfCurrentArchieveExists int= 0;         
          
 DECLARE @flagIfArchieveExists int= 0;          
 SELECT @fasat_Archive_Database_Name= dbo.FN_Get_ArchiveDatabaseName ('VAGENT_ACTIVITY')          
          
    -- To fetch Archive table database  name dynamically       
 SET @archiveYear= CONCAT(@fasat_Archive_Database_Name,'.dbo.agent_activity_archive','_',@year-1)          
 SET @getArchiveCurrentYear = CONCAT(@fasat_Archive_Database_Name,'.dbo.agent_activity_archive','_',@year)            
           
              
 IF OBJECT_ID(@getArchiveCurrentYear,'U') IS NOT NULL            
 BEGIN            
  SET @flagIfCurrentArchieveExists = 1            
 END         
          
           
 DECLARE @gcDesc varchar(50) =(select MSG_TEXT from tbl_online_contents where MSG_ID = 'COMMON_LABEL_GROSS_COM' and LANGUAGE_ID = @locale)          
     ,@ncDesc varchar(50) =(select MSG_TEXT from tbl_online_contents where MSG_ID = 'COMMON_LABEL_NET_COMM' and LANGUAGE_ID = @locale)          
     ,@ovDesc varchar(50) = (select MSG_TEXT from tbl_online_contents where MSG_ID = 'COMMON_LABEL_OVRRIDE_TOTAL' and LANGUAGE_ID = @locale)          
     ,@others varchar(50)=  (select MSG_TEXT from tbl_online_contents where MSG_ID = 'COMMON_MSG_OTHER' and LANGUAGE_ID = @locale)    
           
  ----To calculate Period                  
 DECLARE @day VARCHAR(2) ='01';           
 DECLARE @previous_year_endDt VARCHAR(10);        
        
 DECLARE @period_start_date DATETIME = DATEFROMPARTS(@year, @month, @day);                   
 DECLARE @period_end_date DATETIME =EOMONTH(@period_start_date);                    
                  
 DECLARE @end_Date_Calculation varchar(10) = (Select day (@period_end_date));                  
                  
 --select @period_start_date,@period_end_date                  
                  
 ----To calculate Year To date                  
 DECLARE @current_year_startDt DATETIME = DATEFROMPARTS(@year, '01', '01');                   
 DECLARE @current_year_endDt DATETIME=  DATEFROMPARTS(@year,@month,@end_Date_Calculation);                  
                  
 --select @current_year_startDt,@current_year_endDt                  
                  
 DECLARE @previous_Year varchar(10) = (@year-1);                  
 --PRINT @previous_Year            
         
 --- To calculate Previous Year To date                  
 DECLARE @previous_year_startDt DATETIME = DATEFROMPARTS(@previous_Year, '01', '01') ;   
   
   -- To Handel end date for leap year                  
 IF  ((@YEAR % 4 = 0 AND @YEAR % 100 <> 0) OR @YEAR % 400 = 0 )        
 BEGIN        
 if(@month = '2' OR @month = '02')      
 Begin      
  SET @previous_year_endDt = DATEFROMPARTS(@previous_Year,@month,@end_Date_Calculation-1);      
  END      
  ELSE      
  BEGIN      
   SET @previous_year_endDt  = DATEFROMPARTS(@previous_Year,@month,@end_Date_Calculation);       
   END      
 END        
 ELSE        
 BEGIN        
 SET @previous_year_endDt  = DATEFROMPARTS(@previous_Year,@month,@end_Date_Calculation);          
 END                
                 
                     
          
          
 DECLARE @level_hier VARCHAR(20);                                   
 SELECT @level_hier = (SELECT seqnum FROM tbl_ONLINE_WEBLEVEL WHERE Code=@orgLevel )          
 DECLARE @level_column varchar(20) = CONCAT('mv.mov_org_',@level_hier);           
 --print(@level_column);          
          
          
          
 DECLARE @dynamicSQL NVARCHAR(MAX);           
 DECLARE @where NVARCHAR(MAX) = ' WHERE 1=1 ';           
           
  SET @where = ' WHERE AAC_STATISTICAL_ACCT = 0 and AAC_GEN_SYS not like ''CONV''          
  '          
          
 IF(@division <> '*')          
 BEGIN          
  SET @where = concat(@where ,'And AAC_FASAT_DIVISION  = ''' +@division+''' and GEV_LANG=''' +@locale+'''')          
 END          
          
           
 IF(@lob <> '*')          
 BEGIN          
  SET @where = concat(@where ,'And AAC_FASAT_LOB  = ''' +@lob+''' and GEV_LANG=''' +@locale+'''')          
 END          
          
           
 IF(@product <> '*')          
 BEGIN          
  SET @where = concat(@where ,'And AAC_FASAT_PRODUCT  = ''' +@product+''' and GEV_LANG=''' +@locale+'''')          
 END          
    
  
-- To restrict division to six division for GROSS,NET and OV and not MSC   
 IF(@division = '*')          
 BEGIN      
  SET @where = concat(@where ,'   
    And    
 (  
 (aac_comm_type =''msc'')  
 OR AAC_FASAT_DIVISION in  (''FBSD'',''SEC'',''MSIL'',''MSII'',''WM'',''MB'')  
 )  
      and GEV_LANG=''' +@locale+'''')          
 END          
      
     
          
          
  SET @where = @where + ' AND AAC_COMM_TYPE in (''FYC'',''EFY'',''FOV'',''EOV'',''REN'',''ROV'',''REF'',''MSC'')'          
           
          
 SET @where =  @where + dbo.[FN_GetList_OrgSecurityFilter](@userId, 'mv') + 'AND '+@level_column+'='''+@orgCode+''''          
          
        
        
        
          
DECLARE @from nvarchar(max);          
DECLARE @systemDt DATETIME = (select SOD_SYT_DT FROM SYSTEM_OPTIONS)           
          
SET @from =               
               
  '    
 AND mov_division = ''*''    
 AND MOV_ORG_LEVEL =''AGT''            
 AND (mov_eff_dt <= aac_gen_dt) AND (mov_end_dt >aac_gen_dt OR mov_end_dt is null)            
 INNER JOIN @TempGenEdit on  GEV_VAL=AAC_COMM_TYPE            
 '             
declare @TempSql nvarchar(max)         
SET @TempSql  = N'         
DECLARE @TempGenEdit as Table (  GEV_VAL varchar(10),  GEV_DESC varchar(80), GEV_LANG char(1)  )       
INSERT INTO @TempGenEdit       
SELECT GEV_VAL,GEV_DESC,GEV_LANG      
FROM GEN_EDIT_VALUES       
WHERE GEV_TYPE=''Comm_type'' and GEV_LANG='''+@locale+''' ; '        
          
 IF (@year = YEAR(GETDATE()))                    
 BEGIN              
 IF (@flagIfCurrentArchieveExists = 1 )        
 BEGIN        
        
        
   -- Get data from Agent_Activity, current year archive table and previous year archive table          
 SET @dynamicSql  =  N'        
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                 
                                       
 FROM         dbo.AGENT_ACTIVITY  INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD ' + @from + @where  +            
 ' UNION  ALL                 
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                 
                      FROM '                
  + @archiveYear +' INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD '  + @from  + @where +         
  ' UNION  ALL                 
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                 
                      FROM '                
  + @getArchiveCurrentYear +' INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD '  + @from  + @where          
 END         
 ELSE        
  if (@flagIfCurrentArchieveExists = 0 )        
  BEGIN        
   SET @dynamicSql  =  N'                
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                 
                                       
 FROM         dbo.AGENT_ACTIVITY  INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD ' + @from + @where  +            
 ' UNION  ALL                 
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD ,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                
                      FROM '                
  + @archiveYear +' INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD '  + @from  + @where         
          
  END        
 END        
              
ELSE             
  BEGIN              
 SET @dynamicSql  = N'                
                 
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                
                      FROM '                
  + @archiveYear +' INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD '  + @from + @where+            
  ' UNION ALL              
              
 SELECT  AAC_AGT_CD,AAC_GEN_DT,AAC_COMM_AMT,AAC_COMM_TYPE,AAC_GEN_TRAN,AAC_PREM,AAC_PLAN_CD                
    ,AAC_PAR_CD,GEV_DESC,AAC_ASSIGNMENT_FLG ,AAC_FRM_TO_AGT                 
                      FROM '                
  + @getArchiveCurrentYear +' INNER JOIN MOVEMENT mv on AAC_AGT_CD = mv.MOV_ORG_CD '  + @from  + @where            
              
 END          
           
    IF object_id('tempdb..#base')  is not null drop table #base            
          
          
 CREATE TABLE #base           
 (          
 --id int identity(1,1),          
 AAC_AGT_CD VARCHAR(10),          
 CODE VARCHAR(max),          
 AAC_COMM_TYPE VARCHAR(10),          
 GEV_DESC VARCHAR(80),          
 HIER_2 INT,          
 [COL2_ALIAS] VARCHAR(max),          
 [PERIOD] DECIMAL(19,2),          
 [COL3_ALIAS] VARCHAR(max),          
 [YTD] DECIMAL(19,2),          
 [COL4_ALIAS] VARCHAR(max),          
 [PYTD] DECIMAL(19,2),          
 PLC_PLAN_DESC VARCHAR(50)          
 )          
          
          
          
 DECLARE @dynamicSQLForUnion NVARCHAR(max);          
          
          
          
 DECLARE @dynamicSQL1 NVARCHAR(max);          
        
        
   -- To calculate GROSS, NET and OVERRIDE BAND  AND MSC      
 SET @dynamicSQL1 = @TempSql + N'          
 SELECT AAC_AGT_CD, CODE,AAC_COMM_TYPE,GEV_DESC,HIER_2, [col2_alias], [period],[col3_alias],[YTD]          
 ,[col4_alias],[PYTD],PLC_PLAN_DESC            
 FROM(          
 SELECT *          
 FROM          
 (          
 SELECT AAC_AGT_CD,CODE=CASE WHEN ORG_INDUSTRY_AGT_NUM IS NOT NULL           
       THEN CONCAT(ORG_INDUSTRY_AGT_NUM,''/'',AAC_AGT_CD,'' - '',CLI_CHEQUE_NAME)    ELSE CONCAT(AAC_AGT_CD,'' - '',CLI_CHEQUE_NAME) END          
 ,AAC_COMM_TYPE,GEV_DESC,ISNULL(PLC_PLAN_DESC,'''+@others+''')PLC_PLAN_DESC,          
       [HIER_2]=  CASE WHEN AAC_COMM_TYPE=''FYC'' THEN 5           
                  WHEN AAC_COMM_TYPE=''EFY'' THEN 6          
                  WHEN AAC_COMM_TYPE=''FOV'' THEN 7          
                  WHEN AAC_COMM_TYPE=''EOV'' THEN 8          
                  WHEN AAC_COMM_TYPE=''REN'' THEN 9          
                  WHEN AAC_COMM_TYPE=''ROV'' THEN 10          
                  WHEN AAC_COMM_TYPE=''REF'' THEN 11          
                  WHEN AAC_COMM_TYPE=''MSC'' THEN 12          
                  END,          
 SUM(CASE           
  WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@period_start_date,121)+''' and  '''+CONVERT(VARCHAR(30),@period_end_date,121)+'''          
  THEN aac_prem ELSE 0 END) as gross_period,          
 SUM(CASE           
  WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@current_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@current_year_endDt,121)+'''          
  THEN aac_prem ELSE 0 END) as gross_ytd,          
 SUM(CASE           
  WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@previous_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@previous_year_endDt,121)+'''          
  THEN AAC_PREM ELSE 0 END) as gross_pytd,          
          
 SUM(CASE          
  WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@period_start_date,121)+''' and '''+CONVERT(VARCHAR(30),@period_end_date,121)+'''          
  THEN (CASE             
               WHEN AAC_GEN_TRAN IN (''MFDATA'') THEN AAC_COMM_AMT           
               ELSE 0 end          
              )            
     ELSE 0 END           
    )as net_period,          
          
 SUM(CASE           
  WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@current_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@current_year_endDt,121)+'''          
  THEN(CASE             
               WHEN AAC_GEN_TRAN IN (''MFDATA'') THEN AAC_COMM_AMT           
               ELSE 0 end          
              )            
     ELSE 0 END           
    )as net_ytd,          
          
 SUM(CASE          
   WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@previous_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@previous_year_endDt,121)+'''           
   THEN(CASE             
               WHEN AAC_GEN_TRAN IN (''MFDATA'') THEN AAC_COMM_AMT           
               ELSE 0 end          
              )            
     ELSE 0 END           
    )as net_pytd,      
      
  CASE when '''+@orgLevel +'''=''AGT'' then 0 else (SUM(CASE WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@period_start_date,121)+''' and '''+CONVERT(VARCHAR(30),@period_end_date,121)+'''         
  THEN (CASE WHEN AAC_GEN_TRAN IN (''COMPHIER'') OR       
       (AAC_GEN_TRAN in (''ACTIVITY'',''REV_REPROC'',''REVERSAL'')       
       AND AAC_ASSIGNMENT_FLG in (0,1) AND NOT AAC_FRM_TO_AGT is null  )        
       THEN AAC_COMM_AMT ELSE 0 END)       
  ELSE 0 end)) end AS OT_PERIOD,      
      
  CASE when '''+@orgLevel +'''=''AGT'' then 0 else (SUM(CASE WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@current_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@current_year_endDt,121)+'''         
  THEN (CASE WHEN AAC_GEN_TRAN IN (''COMPHIER'') OR       
       (AAC_GEN_TRAN in (''ACTIVITY'',''REV_REPROC'',''REVERSAL'')       
       AND AAC_ASSIGNMENT_FLG in (0,1) AND NOT AAC_FRM_TO_AGT is null  )        
       THEN AAC_COMM_AMT ELSE 0 END)       
  ELSE 0 end)) end AS OT_YTD,      
      
  CASE when '''+@orgLevel +'''=''AGT'' then 0 else (SUM(CASE WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@previous_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@previous_year_endDt,121)+'''          
  THEN (CASE WHEN AAC_GEN_TRAN IN (''COMPHIER'') OR       
       (AAC_GEN_TRAN in (''ACTIVITY'',''REV_REPROC'',''REVERSAL'')       
       AND AAC_ASSIGNMENT_FLG in (0,1) AND NOT AAC_FRM_TO_AGT is null  )        
       THEN AAC_COMM_AMT ELSE 0 END)       
  ELSE 0 end)) end AS OT_PYTD      
  ,  SUM(case WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@period_start_date,121)+''' and '''+CONVERT(VARCHAR(30),@period_end_date,121)+'''         
  THEN AAC_COMM_AMT else 0 end) msc_period  
    ,SUM(case WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@current_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@current_year_endDt,121)+'''          
  THEN AAC_COMM_AMT else 0 end )msc_ytd  
  ,  SUM(case WHEN AAC_GEN_DT BETWEEN '''+CONVERT(VARCHAR(30),@previous_year_startDt,121)+''' and '''+CONVERT(VARCHAR(30),@previous_year_endDt,121)+'''          
  THEN AAC_COMM_AMT else 0 end) msc_pytd        
          
FROM'          
          
          
           
DECLARE @groupBy varchar(max);          
DECLARE @pivotQuery varchar(max);          
        
          
SET @dynamicSql1 = CONCAT(@dynamicSql1, ' ( ', @dynamicSql,' ) as drv '          
      ,'INNER JOIN ORG on ORG_ORG_CD=AAC_AGT_CD and ORG_ORG_LEVEL=''AGT''          
        INNER JOIN CLIENT on ORG_ACT_CLNT_ID=CLI_CLNT_NUM          
        LEFT JOIN Plan_Code plc on AAC_PLAN_CD= plc.PLC_PLAN_CD and plc.plc_language ='''+@locale+''' ')          
          
SET @groupBy = ' GROUP bY aac_agt_cd,org_industry_agt_num,CLI_CHEQUE_NAME,AAC_COMM_TYPE,AAC_PLAN_CD,PLC_PLAN_DESC,GEV_DESC'           
+')'+'b )d'          
          
SET @dynamicSql1 = CONCAT(@dynamicSql1,@groupBy)          
SET @pivotQuery = ' UNPIVOT          
([period] for COL2_ALIAS in (gross_period,net_period,ot_period,msc_period))as period_table           
UNPIVOT          
([YTD] for COL3_ALIAS in (gross_ytd,net_ytd,ot_ytd,msc_ytd))as YTD_table           
UNPIVOT          
([PYTD] FOR COL4_ALIAS in (gross_pytd,net_pytd,ot_pytd,msc_pytd))as PYTD_table           
WHERE LEFT(col2_alias,3)=LEFT(col3_alias,3) and LEFT(col3_alias,3)=LEFT(col4_alias,3)          
AND (period<>0 OR YTD<>0 OR PYTD<>0)           
'          
          
SET @dynamicSql1 = CONCAT(@dynamicSql1,@pivotQuery)          
          
  
       
PRINT CAST ( @dynamicSQL1 AS ntext  )     
  
  
INSERT INTO #base EXEC sp_executesql @dynamicSQl1;           
          
--select * from #base          
          
 IF object_id('tempdb..#Result')  is not null drop table #Result           
          
 Create Table #Result          
 (          
 [HIER_1] int,          
 [HIER_2] int,          
 [HIER_3] varchar(max),          
 [HIER_4] varchar(max),          
 [Column_Header] varchar(max),          
 [Period] Decimal(19,2),          
 [Year_to_date] DEcimal(19,2),          
 [Previous_year_to_date] Decimal(19,2)   )          
          
 -- Union to SELECT GROSS,NET,OVERRIDE and MSC         
SET @dynamicSQLForUnion = N'          
          
       
SELECT [HIER_1]=CASE WHEN col2_alias=''gross_period'' THEN 1           
      WHEN col2_alias=''net_period'' THEN 2           
      WHEN col2_alias=''ot_period'' THEN 3 end          
 ,[HIER_2]=NULL,[HIER_3]= NULL,[HIER_4]=NULL,          
             [Column_Header]= CASE WHEN col2_alias=''gross_period'' THEN '''+@gcDesc +'''          
                                 WHEN col2_alias=''net_period'' THEN ''' +@ncDesc + '''          
                                 WHEN col2_alias=''ot_period'' THEN '''+@ovDesc+''' END          
 ,SUM(period) as Period, SUM(YTD) as Year_to_date, SUM(PYTD) as Previous_year_to_date           
FROM #base           
WHERE AAC_COMM_TYPE <>''MSC''  and col2_alias!=''msc_period''              
GROUP by col2_alias           
          
UNION          
          
SELECT [HIER_1]=4,HIER_2=NULL,[HIER_3]= NULL,[HIER_4]=NULL,          
 [Column_Header]=GEV_DESC           
 ,SUM(period)as Period, SUM(YTD) as Year_to_date, SUM(PYTD) as Previous_year_to_date           
FROM #base          
WHERE AAC_COMM_TYPE =''MSC''  AND  col2_alias=''msc_period''          
GROUP by col2_alias,GEV_DESC           
HAVING  col2_alias=''msc_period''          
          
Union          
          
SELECT [HIER_1]=CASE WHEN col2_alias=''gross_period'' THEN 1           
         WHEN col2_alias=''net_period'' THEN 2           
      WHEN col2_alias=''ot_period'' THEN 3 end          
 ,HIER_2,[HIER_3]= NULL,[HIER_4]=NULL, [left_column]=GEV_DESC          
 ,SUM(period)as Period,SUM(YTD) as Year_to_date, SUM(PYTD) as Previous_year_to_date            
FROM #base          
WHERE AAC_COMM_TYPE <>''MSC''   and col2_alias!=''msc_period''           
group by col2_alias,HIER_2,GEV_DESC           
          
          
UNION          
          
          
----CODE BLOCK          
           
SELECT [HIER_1]=CASE WHEN col2_alias=''gross_period'' THEN 1           
     WHEN col2_alias=''net_period'' THEN 2           
     WHEN col2_alias=''ot_period'' THEN 3 end          
,HIER_2, [HIER_3]= aac_agt_cd,[HIER_4]=NULL, [left_column]=code          
,SUM(period) as Period,SUM(YTD) as Year_to_date, SUM(PYTD) as Previous_year_to_date           
FROM #base          
GROUP by col2_alias,aac_agt_cd,code,AAC_COMM_TYPE,GEV_DESC,HIER_2   
HAVING AAC_COMM_TYPE <>''MSC''  and col2_alias!=''msc_period''           
          
UNION          
          
SELECT [HIER_1]=4          
 ,HIER_2=aac_agt_cd, [HIER_3]= NULL,[HIER_4]=NULL, [left_column]=code,SUM(period) as Period          
 , SUM(YTD) as Year_to_date, SUM(PYTD) as Previous_year_to_date          
FROM #base          
group by col2_alias,aac_agt_cd,code,AAC_COMM_TYPE,GEV_DESC,HIER_2   
HAVING AAC_COMM_TYPE =''MSC'' and col2_alias=''msc_period''          
          
----PLAN BLOCK          
UNION          
SELECT [HIER_1]=CASE WHEN col2_alias=''gross_period'' THEN 1          
     WHEN col2_alias=''net_period'' THEN 2          
     WHEN col2_alias=''ot_period'' THEN 3 end,          
HIER_2, [HIER_3]= aac_agt_cd, [HIER_4]=PLC_PLAN_DESC,[left_column]=PLC_PLAN_DESC          
,period as Period, YTD as Year_to_date, PYTD as Previous_year_to_date          
FROM #base          
WHERE AAC_COMM_TYPE <>''MSC''  and col2_alias!=''msc_period''            
          
UNION          
          
SELECT [HIER_1]=4,          
 HIER_2=aac_agt_cd, [HIER_3]=PLC_PLAN_DESC, [HIER_4]=PLC_PLAN_DESC,[left_column]=PLC_PLAN_DESC          
 ,period as Period, YTD as Year_to_date, PYTD as Previous_year_to_date           
FROM #base          
WHERE AAC_COMM_TYPE =''MSC'' and col2_alias=''msc_period'''          
          
DECLARE @dynamicSQLWithoutOrderby varchar(max);          
SET @dynamicSQLWithoutOrderby = @dynamicSQLForUnion;          
          
DECLARE @dynamicSQLOrderby varchar(max);          
SET @dynamicSQLOrderby = 'ORDER BY HIER_1, HIER_2, HIER_3,HIER_4 '          
          
SET @dynamicSQLForUnion = @dynamicSQLForUnion + @dynamicSQLOrderby;          
         
          
INSERT INTO #Result EXEC sp_executesql @dynamicSQLForUnion;           
         
       
          
 SET @countQuery = (Select Count(*) FROM #Result);              
         
          
Declare @dynamicSQLForExport varchar(max);          
 ------------------------ To display tree view structure in Excel/csv         
 IF @isTreeView = 0           
 BEGIN          
  SET @dynamicSQLForUnion = CONCAT(@dynamicSQLForUnion,@sorting_sql, @offset_fetch);          
 END          
ELSE          
 BEGIN        
 IF @exportOption = 'CRT'      
 BEGIN      
  SET @exportOption = 'ALL';          
 END      
DECLARE @partitionQuery varchar(max) = 'SELECT CASE WHEN HIER_1 != 0 and HIER_2 is NULL and HIER_3 is NULL and HIER_4 is NULL           
   Then ''>'' +drv.Column_Header          
   WHEN HIER_1 != 0 and HIER_2 is not NULL and HIER_3 is NULL and HIER_4 is NULL           
   Then space(2)+''>''+drv.Column_Header          
   WHEN HIER_1 != 0 and HIER_2 is not NULL and HIER_3 is not NULL and HIER_4 is NULL           
   Then space(5)+''>''+drv.Column_Header          
   WHEN HIER_1 != 0 and HIER_2 is not NULL and HIER_3 is not NULL and HIER_4 is not NULL           
   Then space(10)+''>''+drv.Column_Header          
   END as ''description''          
 , period,year_to_date as [yearToDate]          
 ,previous_year_to_date as [previousYearToDate]          
          
FROM ';          
SET @dynamicSQLForExport = CONCAT(@partitionQuery, ' ( ', @dynamicSQLWithoutOrderby,' ) as drv');          
SET @dynamicSQLForExport = @dynamicSQLForExport + '   ORDER BY drv.HIER_1, drv.HIER_2, drv.HIER_3,drv.HIER_4'          
END          
          
Print '----------------------- '          
          
         
                 
  --Get the value of the result set data count before executing the final resultset query                  
  Declare @RecCount Table (Rec_Count int);            
  Declare @recordCount int, @isLimitExceeded bit;            
          
           
---- IF export max limit exceeded skip the exection (In Framework IF resultset not available data mapping will be excluded)            
 IF @exportOption = 'CRT'            
 BEGIN            
  SET @recordCount = @pageSize          
  SELECT @recordCount;           
  EXEC(@dynamicSQLForExport)          
 END            
 ELSE           
 BEGIN           
           
 Insert Into @RecCount select @countQuery          
  SELECT Top 1 @recordCount = Rec_Count From @RecCount;            
 SELECT @isLimitExceeded = [dbo].[FN_Check_FasatExportMaxLimit] (@recordCount, @exportOption);          
  SELECT @recordCount;           
 IF(@isLimitExceeded = 0)            
  BEGIN            
           
   EXEC(@dynamicSQLForExport);            
  END                
 END          
                            
               
           
          
END          