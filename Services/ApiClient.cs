using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using sistecDesktop.Models;

namespace sistecDesktop.Services
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private readonly string _baseUrl = "http://localhost:3001";

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public ApiClient()
        {
            _cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler()
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ApiResponse> TestConnection()
        {
            try
            {
                Console.WriteLine($"DEBUG: Testando conexão: {_baseUrl}");
                var response = await _httpClient.GetAsync($"{_baseUrl}/");
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Resposta: {content.Substring(0, Math.Min(content.Length, 100))}...");

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode ? "Conexão OK" : "Falha na conexão"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO na conexão: {ex.Message}");
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                var json = JsonConvert.SerializeObject(loginRequest, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var loginUrl = $"{_baseUrl}/api/auth/login";

                Console.WriteLine($"DEBUG: Fazendo login para: {loginUrl}");
                Console.WriteLine($"Dados: {json}");

                var response = await _httpClient.PostAsync(loginUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Resposta completa: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("JSON recebido:");
                    Console.WriteLine(responseBody);

                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody, _jsonSettings);

                    Console.WriteLine($"DEBUG: User ID deserializado: {loginResponse.Data?.User?.IdPerfilUsuario.Id}");
                    Console.WriteLine($"DEBUG: User Name deserializado: {loginResponse.Data?.User?.NomeUsuario}");
                    Console.WriteLine($"DEBUG: Perfil Nivel deserializado: {loginResponse.Data?.User?.IdPerfilUsuario.NivelAcesso}");

                    if (loginResponse.Success)
                    {
                        Console.WriteLine($"Cookies recebidos: {_cookieContainer.Count}");

                        var uri = new Uri(_baseUrl);
                        foreach (Cookie cookie in _cookieContainer.GetCookies(uri))
                        {
                            Console.WriteLine($"   Cookie: {cookie.Name} = {cookie.Value.Substring(0, Math.Min(cookie.Value.Length, 20))}...");
                        }
                    }

                    return loginResponse;
                }
                else
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Erro HTTP {response.StatusCode}: {responseBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCECAO no login: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Erro de exceção: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse> LogoutAsync()
        {
            try
            {
                var logoutUrl = $"{_baseUrl}/api/auth/logout";
                Console.WriteLine($"DEBUG: Fazendo logout: {logoutUrl}");

                var response = await _httpClient.PostAsync(logoutUrl, null);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Logout Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var logoutResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody, _jsonSettings);
                    return logoutResponse;
                }

                return new ApiResponse
                {
                    Success = false,
                    Message = $"Erro no logout: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public void Logout()
        {
            var uri = new Uri(_baseUrl);
            foreach (Cookie cookie in _cookieContainer.GetCookies(uri))
            {
                cookie.Expired = true;
            }

            Console.WriteLine("Cookies locais removidos");
        }

        #region User
        public async Task<UserDatabase> CreateUserAsync(UserDatabase user)
        {
            try
            {
                // Não envie o Id no cadastro!
                var json = JsonConvert.SerializeObject(user, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var usersUrl = $"{_baseUrl}/api/users";
                var response = await _httpClient.PostAsync(usersUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Criando usuário: {json}");
                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody, _jsonSettings);
                    // Retorna UserDatabase já com o id_usuario preenchido (do backend)
                    return apiResponse.Data;
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");

                throw new Exception($"Erro ao criar usuário: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        public async Task<List<PerfilUsuario>> GetPerfisAcessoAsync()
{
    // Chame seu endpoint /api/perfis (ou similar)
    var response = await _httpClient.GetAsync($"{_baseUrl}/api/perfis");
    var json = await response.Content.ReadAsStringAsync();
    var perfis = JsonConvert.DeserializeObject<PerfisApiResponse>(json);
    return perfis.Data;
}


        public async Task<List<UserDatabase>> GetUsersAsync()
        {
            try
            {
                var usersUrl = $"{_baseUrl}/api/users";
                Console.WriteLine($"DEBUG: Buscando usuários: {usersUrl}");

                var response = await _httpClient.GetAsync(usersUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Users Status: {response.StatusCode}");
                Console.WriteLine($"DEBUG: Resposta users (primeiros 300 chars): {responseBody.Substring(0, Math.Min(responseBody.Length, 300))}...");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<UsersResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            Console.WriteLine($"DEBUG: Deserializado {apiResponse.Data.Count} usuários do banco");
                            // Aqui retorna direto UserDatabase, sem mapear para User!
                            return apiResponse.Data;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"DEBUG: Falha ao deserializar users: {ex.Message}");
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                throw new Exception($"Erro ao buscar usuários: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateUserAsync(int id, UserDatabase user)
        {
            var url = $"{_baseUrl}/api/users/{id}";
            var json = JsonConvert.SerializeObject(user, _jsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = responseBody
            };
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                var userUrl = $"{_baseUrl}/api/users/{id}";
                Console.WriteLine($"DEBUG: Buscando usuário: {userUrl}");

                var response = await _httpClient.GetAsync(userUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"DEBUG: User by ID response: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            return new User
                            {
                                IdPerfilUsuario = new PerfilUsuario { Id = apiResponse.Data.PerfilId, Nome = apiResponse.Data.Name, NivelAcesso = apiResponse.Data.NivelAcesso, Descricao = apiResponse.Data.PerfilDescricao },
                                Matricula = apiResponse.Data.Matricula,
                                NomeUsuario = apiResponse.Data.Name,
                                Email = apiResponse.Data.Email,
                                Telefone = apiResponse.Data.Telefone,
                                Setor = apiResponse.Data.Setor,
                                Cargo = apiResponse.Data.Cargo
                            };
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"DEBUG: Falha ao deserializar user: {ex.Message}");
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                throw new Exception($"Erro ao buscar usuário: {response.StatusCode}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }


        public async Task<bool> DeleteUserAsync(int idUsuario, string motivo)
        {
            try
            {
                var url = $"{_baseUrl}/api/users/{idUsuario}";
                Console.WriteLine($"DEBUG: Deletando usuário: {url}");

                var body = new
                {
                    motivo = motivo
                };
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // O HttpClient padrão do .NET não possui DeleteAsync(url, content), então é necessário criar o request manual:
                var request = new HttpRequestMessage(HttpMethod.Delete, url)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                Console.WriteLine($"DeleteUserAsync Status: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição (deletar usuário): {ex.Message}");
            }
        }


        public class DeletedUsersResponse : ApiResponse
        {
            public List<DeletedUserBackup> Data { get; set; }
        }

        public async Task<List<DeletedUserBackup>> GetDeletedUsersAsync()
        {
            try
            {
                var url = $"{_baseUrl}/api/users/deleted";
                Console.WriteLine($"DEBUG: Buscando usuários deletados: {url}");

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"GetDeletedUsersAsync Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {                    
                    var apiResponse = JsonConvert.DeserializeObject<DeletedUsersResponse>(responseBody);
                    // Para compatibilidade com seu pattern de resposta:
                    // Caso use o padrão ApiResponse { bool Success, string Message, List<Data> }
                    // else, ajuste conforme ChamadosResponse, etc.
                    if (apiResponse?.Data != null)
                        return apiResponse.Data;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida.");
                }

                throw new Exception($"Erro ao buscar usuários deletados: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição (obter usuários deletados): {ex.Message}");
            }
        }


        public async Task<bool> RestoreUserAsync(int backupId)
        {
            try
            {
                var url = $"{_baseUrl}/api/users/restore/{backupId}";
                Console.WriteLine($"DEBUG: Restaurando usuário do backup: {url}");

                var response = await _httpClient.PostAsync(url, null); // Sem body necessário nesse caso

                Console.WriteLine($"RestoreUserAsync Status: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição (restaurar usuário): {ex.Message}");
            }
        }

        #endregion


        #region Chamados

        public async Task<List<Chamado>> GetChamadosAsync()
        {
            try
            {

                var chamadosUrl = $"{_baseUrl}/api/chamados";
                Console.WriteLine($"DEBUG: Buscando chamados: {chamadosUrl}");

                var response = await _httpClient.GetAsync(chamadosUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Chamados Status: {response.StatusCode}");

                // MUDAR ESTA LINHA - Ver JSON COMPLETO
                Console.WriteLine($"=== JSON COMPLETO DOS CHAMADOS ===");
                Console.WriteLine(responseBody);
                Console.WriteLine($"==================================");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ChamadosResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            Console.WriteLine($"DEBUG: Deserializado {apiResponse.Data.Count} chamados do banco");

                            var chamados = apiResponse.Data.Select(chamadoDb => new Chamado(chamadoDb)).ToList();

                            Console.WriteLine($"DEBUG: Primeiro chamado - ID: {chamados.FirstOrDefault()?.Id}, Titulo: {chamados.FirstOrDefault()?.Title}");

                            return chamados;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"DEBUG: Falha ao deserializar chamados: {ex.Message}");
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                throw new Exception($"Erro ao buscar chamados: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        public async Task<Chamado> GetChamadoByIdAsync(int id)
        {
            try
            {
                var chamadoUrl = $"{_baseUrl}/api/chamados/{id}";
                var response = await _httpClient.GetAsync(chamadoUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"DEBUG: Chamado by ID response: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ChamadoResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            return new Chamado(apiResponse.Data);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"DEBUG: Falha ao deserializar chamado: {ex.Message}");
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                throw new Exception($"Erro ao buscar chamado: {response.StatusCode}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        public async Task<Chamado> CreateChamadoAsync(CreateChamadoRequest chamado)
        {
            try
            {
                var json = JsonConvert.SerializeObject(chamado, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var chamadosUrl = $"{_baseUrl}/api/chamados";
                var response = await _httpClient.PostAsync(chamadosUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Criando chamado: {json}");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"DEBUG: Create chamado response: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ChamadoResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            var chamadoCriado = new Chamado(apiResponse.Data);
                            Console.WriteLine($"DEBUG: Chamado criado - ID: {chamadoCriado.Id}, Titulo: {chamadoCriado.Title}");
                            return chamadoCriado;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"DEBUG: Falha ao deserializar chamado criado: {ex.Message}");
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                throw new Exception($"Erro ao criar chamado: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        #region Aprovação e rejeição de chamados

        // Buscar chamados pendentes de aprovação
        public async Task<List<Chamado>> GetPendingTickets()
        {
            try
            {
                var url = $"{_baseUrl}/api/chamados/aprovacao";
                Console.WriteLine($"DEBUG: Buscando chamados para aprovação: {url}");

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ChamadosResponse>(responseBody);
                    if (apiResponse?.Data != null)
                    {
                        var chamados = apiResponse.Data.Select(c => new Chamado(c)).ToList();
                        return chamados;
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida.");
                }

                throw new Exception($"Erro ao buscar chamados: {response.StatusCode}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        // Aprovar chamado
        public async Task<bool> AprovarChamadoAsync(int idChamado)
        {
            try
            {
                var url = $"{_baseUrl}/api/chamados/{idChamado}/aprovar";
                Console.WriteLine($"DEBUG: Aprovando chamado: {url}");

                var response = await _httpClient.PostAsync(url, null);

                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        // Rejeitar chamado
        public async Task<bool> RejeitarChamadoAsync(int idChamado, string motivo)
        {
            try
            {
                var url = $"{_baseUrl}/api/chamados/{idChamado}/rejeitar";
                Console.WriteLine($"DEBUG: Rejeitando chamado: {url}");

                var body = new { motivo = motivo };
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        #endregion

        #region Escalonamento de Chamados

        public async Task EscalarChamadoAsync(int idChamado, string motivo)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { motivo }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chamados/{idChamado}/escalar", content);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao escalar chamado: {msg}");
            }
        }

        public async Task<List<ChamadoEscalado>> GetChamadosEscaladosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/chamados/escalados");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChamadosEscaladosResponse>(json);
            return result?.Data ?? new List<ChamadoEscalado>();
        }
        #endregion

        #region Resolução de Chamados

        // Resolve chamados comuns (analista)
        public async Task ResolverChamadoAsync(int idChamado)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chamados/{idChamado}/resolver", null);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao resolver chamado: {msg}");
            }
        }

        // Resolve chamados escalados (gestor/gerente)
        public async Task ResolverChamadoEscaladoAsync(int idChamado)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chamados/{idChamado}/resolver-escalado", null);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao resolver chamado escalado: {msg}");
            }
        }
        #endregion

        #endregion


        public async Task<bool> DeleteChamadoAsync(int id)
        {
            try
            {
                var chamadoUrl = $"{_baseUrl}/api/chamados/{id}";
                var response = await _httpClient.DeleteAsync(chamadoUrl);

                Console.WriteLine($"Deletando chamado {id}");
                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição: {ex.Message}");
            }
        }

        public bool IsAuthenticated()
        {
            var uri = new Uri(_baseUrl);
            var cookies = _cookieContainer.GetCookies(uri);
            return cookies.Count > 0 && !cookies.Cast<Cookie>().All(c => c.Expired);
        }

        public void ShowSessionInfo()
        {
            Console.WriteLine("=== INFO DA SESSÃO ===");
            var uri = new Uri(_baseUrl);
            var cookies = _cookieContainer.GetCookies(uri);

            if (cookies.Count > 0)
            {
                Console.WriteLine($"Cookies ativos: {cookies.Count}");
                foreach (Cookie cookie in cookies)
                {
                    var status = cookie.Expired ? "EXPIRADO" : "ATIVO";
                    Console.WriteLine($"  • {cookie.Name}: {cookie.Value.Substring(0, Math.Min(cookie.Value.Length, 15))}... [{status}]");
                    Console.WriteLine($"    Domínio: {cookie.Domain} | Caminho: {cookie.Path}");
                }
            }
            else
            {
                Console.WriteLine("Nenhuma sessão ativa");
            }
            Console.WriteLine("========================");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
