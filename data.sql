USE [travelRover]
GO
/****** Object:  StoredProcedure [dbo].[SearchEmployees]    Script Date: 10/10/2022 9:32:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SearchEmployees]
 @KwId nvarchar(255),
 @KwName nvarchar(255),
 @KwEmail nvarchar(255),
 @KwPhone nvarchar(255),
 @KwRole nvarchar(255),
 @KwIsActive nvarchar(255)
AS												 
	SELECT *
			FROM dbo.Employees AS e
			WHERE e.IsDelete = 0  AND  ( e.IdEmployee LIKE '%' + @KwId + '%' AND 
									e.NameEmployee LIKE '%' + @KwName + '%' AND
									e.Email LIKE '%' + @KwEmail + '%' AND
									e.Phone LIKE '%' + @KwPhone + '%' AND
									(((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwRole, ',')) = 1 AND e.RoleId LIKE @KwRole + '%') OR
									((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwRole, ',')) > 1 AND e.RoleId IN (SELECT VALUE FROM  STRING_SPLIT(@KwRole, ',')))) AND
									(((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwIsActive, ',')) = 1 AND e.IsActive LIKE @KwIsActive + '%') OR
									((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwIsActive, ',')) > 1 AND e.IsActive IN (SELECT VALUE FROM  STRING_SPLIT(@KwIsActive, ',')))))											 
-- [SearchEmployees] '', '', '', '', '', ''





--USE [travelRover]
--GO
--/****** Object:  StoredProcedure [dbo].[SearchEmployees]    Script Date: 10/10/2022 7:35:32 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER PROCEDURE [dbo].[SearchEmployees]
-- @KwId nvarchar(255),
-- @KwName nvarchar(255),
-- @KwEmail nvarchar(255),
-- @KwPhone nvarchar(255),
-- @KwRole nvarchar(255),
-- @KwIsActive nvarchar(255),
-- @PageNumber int,
-- @PageSize int
--AS
-- DECLARE @Start int, @End int
--	SET @Start = (((@PageNumber - 1) * @PageSize) + 1)
--	SET @End = (@Start + @PageSize - 1)

																 
--	SELECT *
--		FROM (
--			SELECT *, ROW_NUMBER() OVER (ORDER BY RoleId asc) AS RowNum
--			FROM dbo.Employees AS e
--			WHERE e.IsDelete = 0  AND  ( e.IdEmployee LIKE '%' + @KwId + '%' AND 
--									e.NameEmployee LIKE '%' + @KwName + '%' AND
--									e.Email LIKE '%' + @KwEmail + '%' AND
--									e.Phone LIKE '%' + @KwPhone + '%' AND
--									(((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwRole, ',')) = 1 AND e.RoleId LIKE @KwRole + '%') OR
--									((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwRole, ',')) > 1 AND e.RoleId IN (SELECT VALUE FROM  STRING_SPLIT(@KwRole, ',')))) AND
--									(((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwIsActive, ',')) = 1 AND e.IsActive LIKE @KwIsActive + '%') OR
--									((SELECT COUNT(VALUE) FROM  STRING_SPLIT(@KwIsActive, ',')) > 1 AND e.IsActive IN (SELECT VALUE FROM  STRING_SPLIT(@KwIsActive, ',')))))
--		) AS r 
--		WHERE  r.RowNum BETWEEN @Start AND @End
																 
---- [SearchEmployees] '', '', '', '', '', '', 1, 5

   
GO
