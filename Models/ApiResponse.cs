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
        public LoginData Data { get; set; }
    }

    public class LoginData
    {
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




    // Modelo para login (campos limpos)
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("matricula")]
        public string Matricula { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("telefone")]
        public string Telefone { get; set; }

        [JsonProperty("setor")]
        public string Setor { get; set; }

        [JsonProperty("cargo")]
        public string Cargo { get; set; }

        [JsonProperty("id_aprovador")]
        public int? IdAprovador { get; set; }

        [JsonProperty("perfil")]
        public Perfil Perfil { get; set; }
    }

    // Modelo para lista de usuários (campos do banco)
    public class UserDatabase
    {
        [JsonProperty("id_usuario")]
        public int Id { get; set; }
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
        [JsonProperty("tel_usuarios")]
        public string Telefone { get; set; }
        [JsonProperty("id_aprovador_usuario")]
        public int? IdAprovador { get; set; }
        [JsonProperty("id_perfil_usuario")]
        public int PerfilId { get; set; }
        [JsonProperty("nome_perfil")]
        public string PerfilNome { get; set; }
        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }
        [JsonProperty("descricao_perfil_usuario")]
        public string PerfilDescricao { get; set; }


        // Propriedade para compatibilidade
        public Perfil Perfil => new Perfil
        {
            Id = PerfilId,
            Nome = PerfilNome,
            NivelAcesso = NivelAcesso,
            Descricao = PerfilDescricao
        };
    }

    public class DeletedUserBackup
    {
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
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [JsonProperty("descricao")]
        public string Descricao { get; set; }
    }

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

        // Se o backend retornar "status" ou "descricao_status_chamado", você pode mapear ambos:
        [JsonProperty("descricao_status_chamado")]
        public string Status { get; set; }

        // Backend retornou "priority" e também "prioridade_chamado" em alguns pontos.
        // Para garantir compatibilidade, crie propriedades auxiliares e um mapeamento.
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

        // Caso o backend retorne outros campos em português, você pode mantê-los e mapear similarmente.
    }


    // Modelo unificado para exibição
    public class Chamado
    {
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

    public class LoginRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
