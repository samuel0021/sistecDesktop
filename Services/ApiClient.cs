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
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody, _jsonSettings);

                    Console.WriteLine($"DEBUG: User ID deserializado: {loginResponse.Data?.User?.Id}");
                    Console.WriteLine($"DEBUG: User Name deserializado: {loginResponse.Data?.User?.Name}");
                    Console.WriteLine($"DEBUG: Perfil Nivel deserializado: {loginResponse.Data?.User?.Perfil?.NivelAcesso}");

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

        public async Task<List<User>> GetUsersAsync()
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

                            var users = apiResponse.Data.Select(userDb => new User
                            {
                                Id = userDb.Id,
                                Matricula = userDb.Matricula,
                                Name = userDb.Name,
                                Email = userDb.Email,
                                Telefone = userDb.Telefone,
                                Setor = userDb.Setor,
                                Cargo = userDb.Cargo,
                                IdAprovador = userDb.IdAprovador,
                                Perfil = userDb.Perfil
                            }).ToList();

                            Console.WriteLine($"DEBUG: Primeiro usuário - ID: {users.FirstOrDefault()?.Id}, Nome: {users.FirstOrDefault()?.Name}");

                            return users;
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
                                Id = apiResponse.Data.Id,
                                Matricula = apiResponse.Data.Matricula,
                                Name = apiResponse.Data.Name,
                                Email = apiResponse.Data.Email,
                                Telefone = apiResponse.Data.Telefone,
                                Setor = apiResponse.Data.Setor,
                                Cargo = apiResponse.Data.Cargo,
                                IdAprovador = apiResponse.Data.IdAprovador,
                                Perfil = apiResponse.Data.Perfil
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

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                var json = JsonConvert.SerializeObject(user, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var usersUrl = $"{_baseUrl}/api/users";
                var response = await _httpClient.PostAsync(usersUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Criando usuário: {json}");
                Console.WriteLine($"Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<UserResponse>(responseBody, _jsonSettings);
                        if (apiResponse?.Data != null)
                        {
                            return new User
                            {
                                Id = apiResponse.Data.Id,
                                Matricula = apiResponse.Data.Matricula,
                                Name = apiResponse.Data.Name,
                                Email = apiResponse.Data.Email,
                                Telefone = apiResponse.Data.Telefone,
                                Setor = apiResponse.Data.Setor,
                                Cargo = apiResponse.Data.Cargo,
                                IdAprovador = apiResponse.Data.IdAprovador,
                                Perfil = apiResponse.Data.Perfil
                            };
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"DEBUG: Falha ao deserializar user criado: {ex.Message}");
                    }
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida. Faça login novamente.");
                }

                throw new Exception($"Erro ao criar usuário: {response.StatusCode} - {responseBody}");
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

        public async Task<List<Chamado>> GetChamadosAsync()
        {
            try
            {
                var chamadosUrl = $"{_baseUrl}/api/chamados";
                Console.WriteLine($"DEBUG: Buscando chamados: {chamadosUrl}");

                var response = await _httpClient.GetAsync(chamadosUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Chamados Status: {response.StatusCode}");
                Console.WriteLine($"DEBUG: Resposta chamados (primeiros 300 chars): {responseBody.Substring(0, Math.Min(responseBody.Length, 300))}...");

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

        public async Task<bool> UpdateChamadoAsync(int id, Chamado chamado)
        {
            try
            {
                var json = JsonConvert.SerializeObject(chamado, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var chamadoUrl = $"{_baseUrl}/api/chamados/{id}";
                var response = await _httpClient.PutAsync(chamadoUrl, content);

                Console.WriteLine($"Atualizando chamado {id}: {json}");
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
