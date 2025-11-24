using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sistecDesktop.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }

    public class LoginResponse : ApiResponse
    {
        [JsonProperty("data")]
        public LoginData Data { get; set; }
    }

    public class LoginData
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }

    public class UsersResponse : ApiResponse
    {
        public List<UserDatabase> Data { get; set; }
    }

    public class UserResponse : ApiResponse
    {
        public UserDatabase Data { get; set; }
    }

    public class ChamadosResponse : ApiResponse
    {
        public List<ChamadoDatabase> Data { get; set; }
    }

    public class ChamadoResponse : ApiResponse
    {
        public ChamadoDatabase Data { get; set; }
    }


    #region User

    // Modelo para login (campos limpos)
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("matricula")]
        public int Matricula { get; set; }

        [JsonProperty("name")]
        public string NomeUsuario { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("senha")]
        public string Senha { get; set; }

        [JsonProperty("telefone")]
        public string Telefone { get; set; }

        [JsonProperty("setor")]
        public string Setor { get; set; }

        [JsonProperty("cargo")]
        public string Cargo { get; set; }

        [JsonProperty("id_aprovador")]
        public int? IdAprovador { get; set; }

        [JsonProperty("perfil")]
        public PerfilUsuario IdPerfilUsuario { get; set; }

        [JsonProperty("nome")]
        public string PerfilNome { get; set; }

        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [JsonProperty("descricao")]
        public string PerfilDescricao { get; set; }

        [JsonProperty("ramal")]
        public string Ramal { get; set; }

        // Propriedades extras (não vêm do JSON direto, se quiser computed)
        [JsonIgnore]
        public string Nome
        {
            get => NomeUsuario?.Split(' ').FirstOrDefault() ?? "";
            set
            {
                var sobrenomeAtual = Sobrenome ?? "";
                NomeUsuario = $"{value} {sobrenomeAtual}".Trim();
            }
        }

        [JsonIgnore]
        public string Sobrenome
        {
            get => string.Join(" ", (NomeUsuario?.Split(' ').Skip(1) ?? Array.Empty<string>()));
            set
            {
                var nomeAtual = Nome ?? "";
                NomeUsuario = $"{nomeAtual} {value}".Trim();
            }
        }
    }

    // Modelo para lista de usuários (campos do banco)
    public class UserDatabase
    {
        public bool canEdit => App.LoggedUser != null && (App.LoggedUser.IdPerfilUsuario.NivelAcesso >= 4 || App.LoggedUser.Id == this.Id);
        public bool canDelete => App.LoggedUser != null && App.LoggedUser.IdPerfilUsuario.NivelAcesso >= 4 && App.LoggedUser.Id != this.Id;


        [JsonProperty("id_usuario")]
        public int Id { get; set; }
        
        [JsonProperty("matricula_aprovador")]
        public int Matricula { get; set; }

        [JsonProperty("nome_usuario")]
        public string Name { get; set; }

        [JsonProperty("setor_usuario")]
        public string Setor { get; set; }

        [JsonProperty("cargo_usuario")]
        public string Cargo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("tel_usuarios")]
        public string Telefone { get; set; }

        [JsonProperty("id_perfil_usuario")]
        public int PerfilId { get; set; }


        [JsonProperty("nome_perfil")]
        public string PerfilNome { get; set; }


        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [JsonProperty("descricao_perfil_usuario")]
        public string PerfilDescricao { get; set; }

        [JsonProperty("password")]
        public string Senha { get; set; }


        // Propriedade para compatibilidade
        public Perfil Perfil => new Perfil
        {
            Id = PerfilId,
            Nome = PerfilNome,
            NivelAcesso = Matricula,
            Descricao = PerfilDescricao
        };
    }

    public class PerfilUsuario
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [JsonProperty("descricao")]
        public string Descricao { get; set; }
    }

    public class PerfisApiResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public List<PerfilUsuario> Data { get; set; }
    }

    public class DeletedUserBackup
    {
        public bool CanRestore => StatusBackup == "ATIVO";

        [JsonProperty("id_backup")]
        public int BackupId { get; set; }

        [JsonProperty("id_usuario_original")]
        public int UsuarioOriginalId { get; set; }

        [JsonProperty("matricula")]
        public string Matricula { get; set; }

        [JsonProperty("nome_usuario")]
        public string Name { get; set; }

        [JsonProperty("setor_usuario")]
        public string Setor { get; set; }

        [JsonProperty("cargo_usuario")]
        public string Cargo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("senha")]
        public string Senha { get; set; }

        [JsonProperty("tel_usuarios")]
        public string Telefone { get; set; }

        [JsonProperty("id_perfil_usuario")]
        public int PerfilId { get; set; }

        [JsonProperty("id_aprovador_usuario")]
        public int? IdAprovador { get; set; }

        [JsonProperty("fk_chamados_id_chamado")]
        public int? ChamadoRelacionadoId { get; set; }

        [JsonProperty("nome_perfil")]
        public string PerfilNome { get; set; }

        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [JsonProperty("motivo_delecao")]
        public string MotivoDelecao { get; set; }

        [JsonProperty("usuario_que_deletou")]
        public string UsuarioQueDeletou { get; set; }

        [JsonProperty("data_delecao")]
        public DateTime DataDelecao { get; set; }

        [JsonProperty("status_backup")]
        public string StatusBackup { get; set; }

        [JsonProperty("data_restauracao")]
        public DateTime? DataRestauracao { get; set; }

        [JsonProperty("usuario_que_restaurou")]
        public string UsuarioQueRestaurou { get; set; }
    }


    public class Perfil
    {
        [JsonProperty("id_perfil_usuario")]
        public int Id { get; set; }


        [JsonProperty("nome")]
        public string Nome { get; set; }


        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [JsonProperty("descricao_perfil_usuario")]
        public string Descricao { get; set; }
    }

    #endregion

    #region Chamados

    // Modelo para chamados do banco
    public class ChamadoDatabase
    {
        [JsonProperty("id_chamado")]
        public int Id { get; set; }

        // Ajustado para os nomes que o backend retornou
        [JsonProperty("titulo_chamado")]
        public string Title { get; set; }

        [JsonProperty("descricao_detalhada")]
        public string Description { get; set; }

        // Se o backend retornar "status" ou "descricao_status_chamado"
        [JsonProperty("descricao_status_chamado")]
        public string Status { get; set; }

        // Backend retornou "priority" e também "prioridade_chamado" em alguns pontos
        // Para garantir compatibilidade
        [JsonProperty("prioridade_chamado")]
        public int Prioridade { get; set; }

        [JsonProperty("descricao_categoria_chamado")]
        public string Categoria { get; set; }

        [JsonProperty("descricao_problema_chamado")]
        public string Problema { get; set; }

        [JsonProperty("usuario_abertura")]
        public string UsuarioAbertura { get; set; }

        [JsonProperty("email_usuario")]
        public string EmailUsuario { get; set; }

        [JsonProperty("usuario_resolucao")]
        public string UsuarioResolucao { get; set; }

        [JsonProperty("id_usuario_abertura")]
        public int UserId { get; set; }

        [JsonProperty("data_abertura")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("data_resolucao")]
        public DateTime? DataResolucao { get; set; }        
    }

    // Modelo unificado para exibição
    public class Chamado
    {
        public bool CanScale => Status == "Com Analista";

        public bool TicketScaled => Status == "Escalado";

        public bool CanResolve => (Status == "Com Analista" && App.LoggedUser.IdPerfilUsuario.Id >= 2)
                                  || (Status == "Escalado" && App.LoggedUser.IdPerfilUsuario.Id >= 4);

        public bool CanView => (App.LoggedUser.IdPerfilUsuario.Id >= 2) || (App.LoggedUser.Id == UserId);

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Prioridade { get; set; }
        public string Categoria { get; set; }
        public string Problema { get; set; }
        public string UsuarioAbertura { get; set; }
        public string EmailUsuario { get; set; }
        public string UsuarioResolucao { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DataResolucao { get; set; }

        public Chamado() { }

        public Chamado(ChamadoDatabase db)
        {
            Id = db.Id;
            Title = db.Title ?? "Sem título";
            Description = db.Description ?? "";
            Status = db.Status ?? "";
            Prioridade = db.Prioridade;
            Categoria = db.Categoria ?? "";
            Problema = db.Problema ?? "";
            UsuarioAbertura = db.UsuarioAbertura ?? "";
            EmailUsuario = db.EmailUsuario ?? "";
            UsuarioResolucao = db.UsuarioResolucao ?? "";
            UserId = db.UserId;
            CreatedAt = db.CreatedAt;
            DataResolucao = db.DataResolucao;
        }
    }

    #region Escalonamento de chamados

    public class ChamadoEscalado
    {
        [JsonProperty("id_chamado")]
        public int IdChamado { get; set; }

        [JsonProperty("descricao_categoria_chamado")]
        public string DescricaoCategoriaChamado { get; set; }

        [JsonProperty("descricao_problema_chamado")]
        public string DescricaoProblemaChamado { get; set; }

        [JsonProperty("descricao_status_chamado")]
        public string DescricaoStatusChamado { get; set; }

        [JsonProperty("prioridade_chamado")]
        public int PrioridadeChamado { get; set; }

        [JsonProperty("titulo_chamado")]
        public string TituloChamado { get; set; }

        [JsonProperty("data_abertura")]
        public DateTime DataAbertura { get; set; }

        [JsonProperty("usuario_abertura")]
        public string UsuarioAbertura { get; set; }
    }

    public class ChamadosEscaladosResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<ChamadoEscalado> Data { get; set; }
    }

    #endregion

    // carregar listas de categorias e problemas

    public class CategoriaProblema
    {
        public string Categoria { get; set; }
        public string Label { get; set; }
        public List<ProblemaItem> Problemas { get; set; }
    }

    public class ProblemaItem
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class CreateChamadoRequest
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        // não tava conseguindo achar a propriedade certa então coloquei várias
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("descricao_categoria")]
        public string DescricaoCategoria { get; set; }

        [JsonProperty("descricao_categoria_chamado")]
        public string DescricaoCategoriaChamado { get; set; }

        [JsonProperty("descricao_detalhada")]
        public string DescricaoDetalhada { get; set; }

        [JsonProperty("descricao_problema")]
        public string Problem { get; set; } 
    }
    #endregion

    #region Solução IA
    public class RespostaIA
    {

        [JsonProperty("id_resposta_ia")]
        public int IdRespostaIa { get; set; }

        [JsonProperty("fk_chamados_id_chamado")]
        public int ChamadoId { get; set; }

        [JsonProperty("tipo_resposta")]
        public string TipoResposta { get; set; }

        [JsonProperty("analise_triagem")]
        public object AnaliseTriagem { get; set; } // pode criar modelo forte depois

        [JsonProperty("solucao_ia")]
        public string SolucaoIa { get; set; }

        [JsonProperty("feedback_usuario")]
        public string FeedbackUsuario { get; set; }

        [JsonProperty("data_resposta")]
        public DateTime DataResposta { get; set; }

        [JsonProperty("data_feedback")]
        public DateTime? DataFeedback { get; set; }
    }

    public class RespostaIaApiResponse : ApiResponse
    {
        [JsonProperty("data")]
        public RespostaIA Data { get; set; }
    }
    #endregion

    #region Dashboard

    public class AnalystDataItem
    {
        public string nome { get; set; }
        public int chamados { get; set; }
    }

    public class AnalystDataResponse
    {
        public List<AnalystDataItem> data { get; set; }
    }

    public class DashboardStatsResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public DashboardStats data { get; set; }
    }
    public class DashboardStats
    {
        public int abertos { get; set; }
        public int aprovados { get; set; }
        public int rejeitados { get; set; }
        public int triagem_ia { get; set; }
        public int aguardando_resposta { get; set; }
        public int com_analista { get; set; }
        public int resolvidos { get; set; }
        public int fechados { get; set; }
        public int escalados { get; set; }
        public int total { get; set; }
    }

    public class MonthlyDataItem
    {
        public string month { get; set; }
        public int value { get; set; }
    }
    public class MonthlyDataResponse
    {
        public List<MonthlyDataItem> data { get; set; }
    }

    public class CategoryDataItem
    {
        public string name { get; set; }
        public int value { get; set; }
    }
    public class CategoryDataResponse
    {
        public List<CategoryDataItem> data { get; set; }
    }

    public class YearlyDataItem
    {
        public string month { get; set; }
        public int abertos { get; set; }
        public int resolvidos { get; set; }
    }
    public class YearlyDataResponse
    {
        public List<YearlyDataItem> data { get; set; }
    }

    #endregion

    public class LoginRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
