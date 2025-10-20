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

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("tel_usuarios")]
        public string Telefone { get; set; }

        [JsonProperty("setor_usuario")]
        public string Setor { get; set; }

        [JsonProperty("cargo_usuario")]
        public string Cargo { get; set; }

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

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status_chamado")]
        public string Status { get; set; }

        [JsonProperty("id_usuario_abertura")]
        public int UserId { get; set; }

        [JsonProperty("user_id")]
        public int? UserIdAlternative { get; set; }

        [JsonProperty("data_abertura")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("data_ultima_atualizacao")]
        public DateTime? UpdatedAt { get; set; }

        // Para compatibilidade com o código existente
        public int GetUserId() => UserIdAlternative ?? UserId;
    }

    // Modelo unificado para exibição
    public class Chamado
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Construtor para conversão
        public Chamado() { }

        public Chamado(ChamadoDatabase db)
        {
            Id = db.Id;
            Title = db.Title ?? "";
            Description = db.Description ?? "";
            Status = db.Status ?? "";
            UserId = db.GetUserId();
            CreatedAt = db.CreatedAt;
            UpdatedAt = db.UpdatedAt;
        }
    }

    public class CreateChamadoRequest
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }

    public class LoginRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
