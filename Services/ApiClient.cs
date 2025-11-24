using Newtonsoft.Json;
using sistecDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sistecDesktop.Services
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;        // objeto para fazer as requisições HTTP
        private readonly CookieContainer _cookieContainer;      // gerencia os cookies para autenticar automaticamente
        private readonly string _baseUrl = "http://localhost:3001";     // URL base da API

        // Configurações para serialização/deserialização JSON (ignora nulos, datas no formato ISO)
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        // Inicializa a instância da classe
        #region Construtor
        public ApiClient()
        {
            _cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler()
            {
                CookieContainer = _cookieContainer,
                UseCookies = true
            };

            _httpClient = new HttpClient(handler);      // Associa esse handler ao HttpClient principal                                                
            _httpClient.DefaultRequestHeaders.Accept.Clear();        // Limpa configurações de Accept
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));       // Define apenas respostas no formato JSON

            // Define timeout padrão das requisições pra 30 segundos
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        #endregion

        // Método pra testar conexão, caso necessário
        #region Teste de Conexão
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
        #endregion

        // Métodos e Tasks
        #region Login e Logout
        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                // Serializa o objeto loginRequest pra JSON
                var json = JsonConvert.SerializeObject(loginRequest, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Monta a URL do endpoint de login
                var loginUrl = $"{_baseUrl}/api/auth/login";

                Console.WriteLine($"DEBUG: Fazendo login para: {loginUrl}");
                Console.WriteLine($"Dados: {json}");

                // Envia a requisição POST para o endpoint de login com o JSON
                var response = await _httpClient.PostAsync(loginUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Resposta completa: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    // Se a resposta for 2xx, deserializa para LoginResponse
                    Console.WriteLine("JSON recebido:");
                    Console.WriteLine(responseBody);
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody, _jsonSettings);

                    Console.WriteLine($"DEBUG: User ID deserializado: {loginResponse.Data?.User?.IdPerfilUsuario.Id}");
                    Console.WriteLine($"DEBUG: User Name deserializado: {loginResponse.Data?.User?.NomeUsuario}");
                    Console.WriteLine($"DEBUG: Perfil Nivel deserializado: {loginResponse.Data?.User?.IdPerfilUsuario.NivelAcesso}");

                    if (loginResponse.Success)
                    {
                        // Se o login foi aceito, mostra quantos cookies foram recebidos
                        Console.WriteLine($"Cookies recebidos: {_cookieContainer.Count}");
                        var uri = new Uri(_baseUrl);
                        foreach (Cookie cookie in _cookieContainer.GetCookies(uri))
                        {
                            // Mostra nome e início do valor de cada cookie guardado
                            Console.WriteLine($"   Cookie: {cookie.Name} = {cookie.Value.Substring(0, Math.Min(cookie.Value.Length, 20))}...");
                        }
                    }
                    return loginResponse;
                }
                else
                {
                    // Erro HTTP: retorna um LoginResponse indicando falha e mostra a mensagem de erro
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Erro HTTP {response.StatusCode}: {responseBody}"
                    };
                }
            }
            catch (Exception ex)
            {
                // Captura e retorna qualquer exceção como falha no login
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
                // Monta a URL do endpoint de logout
                var logoutUrl = $"{_baseUrl}/api/auth/logout";
                Console.WriteLine($"DEBUG: Fazendo logout: {logoutUrl}");

                // Envia a requisição POST para o logout
                var response = await _httpClient.PostAsync(logoutUrl, null);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Logout Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    // Se for sucesso, tenta deserializar para ApiResponse
                    var logoutResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody, _jsonSettings);
                    return logoutResponse;
                }

                // Retorna objeto de resposta indicando falha
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Erro no logout: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                // Qualquer exceção, retorna resposta de erro
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
                cookie.Expired = true;  // Marca todos cookies dessa URL como expirados (faz logout local)
            }
            Console.WriteLine("Cookies locais removidos");
        }

        #endregion

        // Métodos e Tasks do Usuário
        #region Usuário
        public async Task<UserDatabase> CreateUserAsync(UserDatabase user)
        {
            try
            {
                // Serializa o objeto user para JSON
                var json = JsonConvert.SerializeObject(user, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Endpoint para criar usuário novo na API
                var usersUrl = $"{_baseUrl}/api/users";
                // Envia POST para a rota '/api/users' com JSON no corpo
                var response = await _httpClient.PostAsync(usersUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Criando usuário: {json}");
                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    // Se sucesso, deserializa resposta para UserResponse e retorna o usuário criado.
                    var apiResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody, _jsonSettings);
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
            // Consulta endpoint de perfis de acesso
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/perfis");
            var json = await response.Content.ReadAsStringAsync();
            // Desserializa resultado para objeto de resposta
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
                        // Desserializa resposta para lista de usuários
                        var apiResponse = JsonConvert.DeserializeObject<UsersResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            Console.WriteLine($"DEBUG: Deserializado {apiResponse.Data.Count} usuários do banco");
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
            // Monta a URL do usuário a ser atualizado pelo id
            var url = $"{_baseUrl}/api/users/{id}";
            var json = JsonConvert.SerializeObject(user, _jsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                // Retorna se foi sucesso ou não
                Success = response.IsSuccessStatusCode,
                Message = responseBody
            };
        }

        public async Task<bool> DeleteUserAsync(int idUsuario, string motivo)
        {
            try
            {
                // Constrói URL de deletar e prepara corpo com motivo
                var url = $"{_baseUrl}/api/users/{idUsuario}";
                Console.WriteLine($"DEBUG: Deletando usuário: {url}");

                var body = new
                {
                    motivo = motivo
                };
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Cria request manual (HttpClient não tem DeleteAsync(url, content))
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

                // Envia POST sem body para o endpoint de restauração
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

        // Métodos e Tasks dos Chamados
        #region Chamados

        public async Task<List<Chamado>> GetChamadosAsync()
        {
            try
            {
                // Monta URL de busca
                var chamadosUrl = $"{_baseUrl}/api/chamados";
                Console.WriteLine($"DEBUG: Buscando chamados: {chamadosUrl}");

                var response = await _httpClient.GetAsync(chamadosUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Chamados Status: {response.StatusCode}");

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
                // Monta URL de busca por ID
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
                // Serializa o objeto chamado para JSON
                var json = JsonConvert.SerializeObject(chamado, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Monta URL de POST do chamado
                var chamadosUrl = $"{_baseUrl}/api/chamados";
                var response = await _httpClient.PostAsync(chamadosUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"DEBUG: Criando chamado: {json}");
                Console.WriteLine($"DEBUG: Status: {response.StatusCode}");
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

        // Resolve chamados comuns
        public async Task ResolverChamadoAsync(int idChamado, string motivoResolucao)
        {
            var body = new
            {
                id_chamado = idChamado,
                relatorio_resposta = motivoResolucao
            };
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chamados/resolver-com-relatorio", content);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao resolver chamado: {msg}");
            }
        }

        // Resolve chamados escalados
        public async Task ResolverChamadoEscaladoAsync(int idChamado, string motivoResolucao)
        {
            var body = new
            {
                id_chamado = idChamado,
                relatorio_resposta = motivoResolucao
            };
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chamados/{idChamado}/resolver-escalado", content);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao resolver chamado escalado: {msg}");
            }
        }
        #endregion

        #endregion

        // Solução IA
        #region SoluçãoIA
        public async Task<RespostaIA> GetSolucaoIaAsync(int idChamado)
        {
            var url = $"{_baseUrl}/api/chamados/{idChamado}/solucao-ia";
            var response = await _httpClient.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao buscar solução IA: {response.StatusCode} - {responseBody}");

            var apiResponse = JsonConvert.DeserializeObject<RespostaIaApiResponse>(responseBody, _jsonSettings);
            return apiResponse?.Data;
        }

        public async Task EnviarFeedbackIaAsync(int idChamado, string feedback)
        {
            var url = $"{_baseUrl}/api/chamados/{idChamado}/feedback-ia";
            var body = new { feedback }; // "DEU_CERTO" ou "DEU_ERRADO"
            var json = JsonConvert.SerializeObject(body, _jsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao enviar feedback IA: {response.StatusCode} - {responseBody}");
        }
        #endregion

        // Métodos e Tasks do Dashboard
        #region Dashboard

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            try
            {
                // Monta URL de estatísticas
                var url = $"{_baseUrl}/api/estatisticas/dashboard-stats";
                Console.WriteLine($"DEBUG: Buscando dashboard stats: {url}");
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<DashboardStatsResponse>(responseBody, _jsonSettings);
                    return apiResponse?.data ?? new DashboardStats();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                throw new Exception($"Erro ao buscar stats: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex) { throw new Exception($"Erro na requisição: {ex.Message}"); }
        }

        public async Task<List<MonthlyDataItem>> GetMonthlyDataAsync()
        {
            try
            {
                // Busca chamados mensais
                var url = $"{_baseUrl}/api/estatisticas/chamados-mensais";
                Console.WriteLine($"DEBUG: Buscando dados mensais: {url}");
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<MonthlyDataResponse>(responseBody, _jsonSettings);
                    return apiResponse?.data ?? new List<MonthlyDataItem>();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                throw new Exception($"Erro ao buscar dados mensais: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex) { throw new Exception($"Erro na requisição: {ex.Message}"); }
        }

        public async Task<List<CategoryDataItem>> GetCategoryDataAsync()
        {
            try
            {
                // Busca chamados por categoria
                var url = $"{_baseUrl}/api/estatisticas/chamados-categoria";
                Console.WriteLine($"DEBUG: Buscando categorias: {url}");
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<CategoryDataResponse>(responseBody, _jsonSettings);
                    return apiResponse?.data ?? new List<CategoryDataItem>();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                throw new Exception($"Erro ao buscar categorias: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex) { throw new Exception($"Erro na requisição: {ex.Message}"); }
        }

        public async Task<List<YearlyDataItem>> GetYearlyDataAsync()
        {
            try
            {
                // Busca chamados anuais
                var url = $"{_baseUrl}/api/estatisticas/chamados-anuais";
                Console.WriteLine($"DEBUG: Buscando dados anuais: {url}");
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<YearlyDataResponse>(responseBody, _jsonSettings);
                    return apiResponse?.data ?? new List<YearlyDataItem>();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                throw new Exception($"Erro ao buscar dados anuais: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex) { throw new Exception($"Erro na requisição: {ex.Message}"); }
        }

        public async Task<List<AnalystDataItem>> GetAnalystDataAsync()
        {
            try
            {
                // Busca chamados com analistas
                var url = $"{_baseUrl}/api/estatisticas/chamados-analistas";
                Console.WriteLine($"DEBUG: Buscando chamados por analista: {url}");
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<AnalystDataResponse>(responseBody, _jsonSettings);
                    return apiResponse?.data ?? new List<AnalystDataItem>();
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                throw new Exception($"Erro ao buscar analistas: {response.StatusCode} - {responseBody}");
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (Exception ex) { throw new Exception($"Erro na requisição: {ex.Message}"); }
        }

        #endregion

        // Métodos não referenciados para debug, caso necessário
        #region Debug
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
        #endregion
    }
}
