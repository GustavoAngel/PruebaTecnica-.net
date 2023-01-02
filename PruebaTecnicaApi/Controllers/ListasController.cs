using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PruebaTecnicaApi.Controllers
{
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ListasController : ControllerBase
    {
        string strConString = "workstation id=apipruebatecnica.mssql.somee.com;packet size=4096;user id=estefanykumi_SQLLogin_1;pwd=mx8tmhpeup;data source=apipruebatecnica.mssql.somee.com;persist security info=False;initial catalog=apipruebatecnica";
        // GET: api/<ListasController>
        [HttpGet]
        public IEnumerable<Model.Meta> ObtenerMetas()
        {
            var MetasList = new List<Model.Meta>();
            using (var con = new SqlConnection(strConString))
            {
                con.Open();
                string query = @"
                              		 SELECT[id]
                                      ,[Nombre]
                                      ,[FechaCreacion]
                                      ,[TotalTareas]
                                      ,porciento
									  ,COALESCE(c.completadas, 0)
                                  FROM[dbo].[Metas] left join (
								  select Metas.id as idAux, COUNT(Tareas.Nombre) as completadas
								  from tareas, metas where Tareas.IdMeta= Metas.id
								  and estado='Completada'
								  group by Metas.id) as c

								  on c.idAux= Metas.id
                                ";

                var cmd = new SqlCommand(query, con);

                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                        var metaAux = new Model.Meta()
                        {
                            id =Guid.Parse(dr[0].ToString()),
                            Nombre = dr[1].ToString(),
                            FechaCreacion = DateTime.Parse(dr[2].ToString()),
                            TotalTareas = int.Parse(dr[3].ToString()),
                            Porciento = int.Parse(dr[4].ToString()),
                            Completadas = int.Parse(dr[5].ToString()),
                        };
                    if (metaAux.Completadas == 0 || metaAux.TotalTareas==0)
                    {
                        metaAux.Porciento = 0;
                    }
                    else
                    {
                       metaAux.Porciento = metaAux.Completadas*100/metaAux.TotalTareas;
                    }
                        
                    MetasList.Add(metaAux);
                }
                con.Close();
            }
            return MetasList;
        }

        public bool ExisteMeta(string nombre)
        {
            var listaclientes = new List<Model.Meta>();
            using (var con = new SqlConnection(strConString))
            {
                con.Open();
                string query = @"
                                SELECT count(Nombre) as conteo
                                  FROM[dbo].[Metas]
                                where nombre = @Nombre
                                ";

                var cmd = new SqlCommand(query, con);

                cmd.Parameters.Add(new SqlParameter("@Nombre", nombre));

                var dr =int.Parse(cmd.ExecuteScalar().ToString());
                
                con.Close();
                if (dr>0)
                {
                    return true;
                }
                return false;
            }
            //return listaclientes;
        }

        [Produces("application/json")]
        public string AddMeta([FromBody] JsonElement meta)
        {
            try
            {
                var resulkt = meta.GetString();

                var oMeta = JsonSerializer.Deserialize<Model.Meta>(resulkt);
                if (!ExisteMeta(oMeta.Nombre))
                {
                    using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();

                    string query = @"
                            INSERT INTO [dbo].[Metas]
                                       ([id]
                                       ,[Nombre]
                                       ,[FechaCreacion]
                                       ,[TotalTareas]
                                       ,[Porciento])
                                 VALUES
                                       (NewId()
                                       ,@Nombre
                                       ,@fecha
                                       ,0
                                       ,0)
                            ";

                    var cmd = new SqlCommand(query, con);

                    cmd.Parameters.Add(new SqlParameter("@Nombre",oMeta.Nombre));
                    cmd.Parameters.Add(new SqlParameter("@Fecha", oMeta.FechaCreacion));

                    var dr = cmd.ExecuteNonQuery();

                    con.Close();
                }

                return "201 CREATED";
                }
                else
                {
                    return "VALUE EXIST";
                }
            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";
                
            }                   
        }

        // DELETE api/<ListasController>/
        [Produces("application/json")]
        public string DeleteMeta(string id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();

                    string query = @"
                           DELETE FROM [dbo].[Tareas] where idmeta=@id;
                           DELETE FROM [dbo].[Metas]
                            WHERE id=@id
                            ";

                    var cmd = new SqlCommand(query, con);

                    cmd.Parameters.Add(new SqlParameter("@id", id));                    

                    var dr = cmd.ExecuteNonQuery();

                    con.Close();
                }

                return "201 CREATED";
            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }

        [Produces("application/json")]
        public string DeleteTarea([FromBody] JsonElement Tareas)
        {
            var peticion = Tareas.GetString();

            var tareasList = JsonSerializer.Deserialize<Model.Tarea[]>(peticion);

            try
            {
                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();

                    foreach (var item in tareasList)
                    {
                        string query = @"
                           DELETE FROM [dbo].[tareas]
                            WHERE id=@id
                            ";

                        var cmd = new SqlCommand(query, con);

                        cmd.Parameters.Add(new SqlParameter("@id", item.Id));

                        var dr = cmd.ExecuteNonQuery();
                    }                   

                    con.Close();
                }

                return "201 CREATED";
            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }

        [Produces("application/json")]
        public string CompletarTareas([FromBody] JsonElement Tareas)
        {
            var peticion = Tareas.GetString();

            var tareasLista = JsonSerializer.Deserialize<Model.Tarea[]>(peticion);

            try
            {
                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();

                    foreach (var item in tareasLista)
                    {
                        string query = @"
                                UPDATE [dbo].[Tareas]
                                SET Estado = 'Completada'
                                WHERE id=@id
                            ";

                        var cmd = new SqlCommand(query, con);

                        cmd.Parameters.Add(new SqlParameter("@id", item.Id));

                        var dr = cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }

                return "201 CREATED";
            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }

        [Produces("application/json")]
        public string TareaImportante([FromBody] JsonElement Tareas)
        {
            var peticion = Tareas.GetString();

            var oTarea = JsonSerializer.Deserialize<Model.Tarea>(peticion);

            try
            {
                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();

                    
                        string query = @"
                                UPDATE [dbo].[Tareas]
                                SET IsImportant = @IsImportant
                                WHERE id=@id
                            ";

                        var cmd = new SqlCommand(query, con);

                        cmd.Parameters.Add(new SqlParameter("@id", oTarea.Id));
                        cmd.Parameters.Add(new SqlParameter("@IsImportant", oTarea.IsImportant));

                        var dr = cmd.ExecuteNonQuery();

                    con.Close();
                }

                return "201 CREATED";
            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }


        // DELETE api/<ListasController>/
        [Produces("application/json")]
        public string UpdateMeta(string id, [FromBody] JsonElement oMeta)
        {
            try
            {
                var resulkt = oMeta.GetString();

                var oMetad = JsonSerializer.Deserialize<Model.Meta>(resulkt);

                if (!ExisteMeta(oMetad.Nombre))
                {
                    using (SqlConnection con = new SqlConnection(strConString))
                    {
                        con.Open();

                        string query = @"
                                UPDATE [dbo].[Metas]
                                SET [Nombre] = @nombre
                                WHERE id = @id
                            ";

                        var cmd = new SqlCommand(query, con);

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@nombre", oMetad.Nombre));

                        var dr = cmd.ExecuteNonQuery();

                        con.Close();
                    }

                    return "201 CREATED";
                }
                else
                {
                    return "VALUE EXIST";
                }
             
            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }

        [Produces("application/json")]
        public string UpdateTarea(string id, [FromBody] JsonElement oTarea)
        {
            try
            {
                var resulkt = oTarea.GetString();

                var tareaAux = JsonSerializer.Deserialize<Model.Tarea>(resulkt);

                if (!ExisteMeta(tareaAux.Nombre))
                {
                    using (SqlConnection con = new SqlConnection(strConString))
                    {
                        con.Open();

                        string query = @"
                                UPDATE [dbo].[Tareas]
                                SET [Nombre] = @nombre
                                WHERE id = @id
                            ";

                        var cmd = new SqlCommand(query, con);

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@nombre", tareaAux.Nombre));

                        var dr = cmd.ExecuteNonQuery();

                        con.Close();
                    }

                    return "201 CREATED";
                }
                else
                {
                    return "VALUE EXIST";
                }

            }
            catch (Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }


        #region Servicios tareas
        [Produces("application/json")]
        public string AddTarea([FromBody] JsonElement meta)
        {
            try
            {
                var resulkt = meta.GetString();

                var oMeta = JsonSerializer.Deserialize<Model.Tarea>(resulkt);

                using (SqlConnection con = new SqlConnection(strConString))
                {
                    con.Open();

                    string query = @"
                                INSERT INTO [dbo].[Tareas]
                                ([id]
                                ,[IdMeta]
                                ,[Nombre]
                                ,[FechaCreada]
                                ,[Estado]
                                ,[IsImportant])
                                VALUES
                                (NewId()
                                ,@IdMeta
                                ,@Nombre
                                ,@Fecha
                                ,@Estado
                                ,0)   
                                update metas set totaltareas = totaltareas+1 where id =@IdMeta
                            ";

                    var cmd = new SqlCommand(query, con);

                    cmd.Parameters.Add(new SqlParameter("@IdMeta", oMeta.IdMeta));
                    cmd.Parameters.Add(new SqlParameter("@Nombre", oMeta.Nombre));
                    cmd.Parameters.Add(new SqlParameter("@Fecha", oMeta.FechaCreada));
                    cmd.Parameters.Add(new SqlParameter("@Estado", "Abierta"));

                    var dr = cmd.ExecuteNonQuery();

                    con.Close();
                }

                return "201 CREATED";
            }
            catch(Exception ex)
            {
                return "500 INTERNAL SERVER ERROR";

            }
        }

        [HttpGet]
        public IEnumerable<Model.Tarea> ObtenerTareas(string id)
        {
            var TareasList = new List<Model.Tarea>();
            using (var con = new SqlConnection(strConString))
            {
                con.Open();
                string query = @"
                                SELECT [id]
                                      ,[IdMeta]
                                      ,[Nombre]
                                      ,[FechaCreada]
                                      ,[Estado]
                                      ,[IsImportant]
                                  FROM [dbo].[Tareas]
                                  where idMeta=@Idmeta
                                ";

                var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add(new SqlParameter("Idmeta", id));
                var dr = cmd.ExecuteReader();
                
                while (dr.Read())
                {
                    var tareaAux = new Model.Tarea()
                    {
                        Id = Guid.Parse(dr[0].ToString()),
                        IdMeta= Guid.Parse(dr[1].ToString()),
                        Nombre = dr[2].ToString(),
                        FechaCreada = DateTime.Parse(dr[3].ToString()),
                        Estado = dr[4].ToString(),
                        IsImportant = Boolean.Parse(dr[5].ToString())
                    };

                    TareasList.Add(tareaAux);
                }
                con.Close();
            }
            return TareasList;
        }

        [HttpGet]
        public IEnumerable<Model.Tarea> ObtenerTareasPaginadas(string id,int inicio, int fin)
        {
            var TareasList = new List<Model.Tarea>();
            using (var con = new SqlConnection(strConString))
            {
                con.Open();
                string query = @"
                              WITH C AS
                            ( 
                                SELECT [id]
                                ,[IdMeta]
                                ,[Nombre]
                                ,[FechaCreada]
                                ,[Estado]
                                ,[IsImportant]
                                ,ROW_NUMBER() OVER(ORDER BY id)  as rownum
                                FROM [dbo].[Tareas]
                                where idMeta=@Idmeta
                            )
                            SELECT [id]
                                ,[IdMeta]
                                ,[Nombre]
                                ,[FechaCreada]
                                ,[Estado]
                                ,[IsImportant]
                            FROM C
                            WHERE rownum BETWEEN @inicio AND @fin
                                                            ";

                var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add(new SqlParameter("Idmeta", id));
                cmd.Parameters.Add(new SqlParameter("inicio", inicio));
                cmd.Parameters.Add(new SqlParameter("fin", fin));
                var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var tareaAux = new Model.Tarea()
                    {
                        Id = Guid.Parse(dr[0].ToString()),
                        IdMeta = Guid.Parse(dr[1].ToString()),
                        Nombre = dr[2].ToString(),
                        FechaCreada = DateTime.Parse(dr[3].ToString()),
                        Estado = dr[4].ToString(),
                        IsImportant = Boolean.Parse(dr[5].ToString())
                    };

                    TareasList.Add(tareaAux);
                }
                con.Close();
            }
            return TareasList;
        }

        [HttpGet]
        public IEnumerable<Model.Tarea> ObtenerTareasPaginadasFiltros(string id, int inicio, int fin,string nombre, DateTime? fecha, string estado)
        {
            var TareasList = new List<Model.Tarea>();
            using (var con = new SqlConnection(strConString))
            {
                con.Open();
                string query = @"
                              WITH C AS
                            ( 
                                SELECT [id]
                                ,[IdMeta]
                                ,[Nombre]
                                ,[FechaCreada]
                                ,[Estado]
                                ,[IsImportant]
                                ,ROW_NUMBER() OVER(ORDER BY id)  as rownum
                                FROM [dbo].[Tareas]
                                where idMeta=@Idmeta
                ";
                if (nombre != null && nombre.Trim()!="")
                {
                    query += " and nombre like '%"+nombre+"%'";
                }
                if (fecha!=null)
                {
                    query += " and FechaCreada = @fecha";
                }
                if (estado!=null && estado!="")
                {
                    query += " and Estado = '%"+estado+"%'";
                }
                query += @"
                            )
                            SELECT [id]
                                ,[IdMeta]
                                ,[Nombre]
                                ,[FechaCreada]
                                ,[Estado]
                                ,[IsImportant]
                            FROM C
                            WHERE rownum BETWEEN @inicio AND @fin
                                                            ";

                var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add(new SqlParameter("Idmeta", id));
                cmd.Parameters.Add(new SqlParameter("inicio", inicio));
                cmd.Parameters.Add(new SqlParameter("fin", fin));
               
                if (fecha != null)
                {
                    cmd.Parameters.Add(new SqlParameter("fecha", fecha));
                }
           
                var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var tareaAux = new Model.Tarea()
                    {
                        Id = Guid.Parse(dr[0].ToString()),
                        IdMeta = Guid.Parse(dr[1].ToString()),
                        Nombre = dr[2].ToString(),
                        FechaCreada = DateTime.Parse(dr[3].ToString()),
                        Estado = dr[4].ToString(),
                        IsImportant = Boolean.Parse(dr[5].ToString())
                    };

                    TareasList.Add(tareaAux);
                }
                con.Close();
            }
            return TareasList;
        }

        [HttpGet]
        public int ObtenerTotalTareas(string id, string nombre, DateTime? fecha, string estado)
        {
            var TareasList = new List<Model.Tarea>();
            using (var con = new SqlConnection(strConString))
            {
                con.Open();
                string query = @"
                                SELECT COUNT(Nombre)
                                  FROM [dbo].[Tareas]
                                  where idMeta=@Idmeta
                                ";
                if (nombre != null && nombre.Trim() != "")
                {
                    query += " and nombre like '%" + nombre + "%'";
                }
                if (fecha != null)
                {
                    query += " and FechaCreada = @fecha";
                }
                if (estado != null && estado != "")
                {
                    query += " and Estado = '%" + estado + "%'";
                }
                var cmd = new SqlCommand(query, con);
                cmd.Parameters.Add(new SqlParameter("Idmeta", id));
                if (fecha != null)
                {
                    cmd.Parameters.Add(new SqlParameter("fecha", fecha));
                }
                var total =int.Parse(cmd.ExecuteScalar().ToString());
                                
                con.Close();

                return total;
            }
            
        }
        
        #endregion


    }
}
