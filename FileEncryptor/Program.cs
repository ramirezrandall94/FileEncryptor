// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

FileEncryptor.Program.Main(new string[0]);

namespace FileEncryptor
{
    public class Program
    {
        class FilesEncryptor
        {
            class FilePath
            {
                public string relative_path = string.Empty;
                public string full_path = string.Empty;
            }
            static string file_content_separator = "a";
            static string folder_path_separator = "b";
            static string file_path_separator = "e";
            static string path_type_separator = "c";
            static string byte_int_separator = "f";
            static string paths_to_content_separator = "d";
            public void Encrypt(string folder_path,
                string output_folder,
                string password, string private_key)
            {
                if (folder_path[folder_path.Length - 1] != '\\')
                {
                    folder_path = folder_path + "\\";
                }
                if (output_folder[output_folder.Length - 1] != '\\')
                {
                    output_folder = output_folder + "\\";
                }
                List<string> all_directories_paths = new List<string>();
                List<FilePath> all_files_paths = new List<FilePath>();
                GetAllPaths(folder_path, null, ref all_directories_paths,
                    ref all_files_paths);
                string paths = BuildPaths(all_directories_paths,
                    all_files_paths, password, private_key);
                string all_files_contents = string.Empty;
                for (int filePath_index = 0;
                    filePath_index < all_files_paths.Count;
                    ++filePath_index)
                {
                    FilePath filePath = all_files_paths[filePath_index];
                    byte[] file_bytes = File.ReadAllBytes(filePath.full_path);
                    string file_contents = string.Empty;
                    foreach (byte current_byte in file_bytes)
                    {
                        char current_char = (char)current_byte;
                        file_contents += current_char;
                    }
                    string encrypted_file_content =
                        EncryptString(file_contents, password, private_key);
                    all_files_contents += encrypted_file_content;
                    if (filePath_index <
                        (all_files_paths.Count - 1))
                    {
                        all_files_contents += file_content_separator;
                    }
                }
                string encrypted_file_contents = paths +
                    paths_to_content_separator + all_files_contents;
                File.WriteAllText(output_folder + "encrypted.e",
                    encrypted_file_contents,
                    Encoding.Unicode);
            }
            public void Decrypt(string encrypted_file_path,
                string output_folder,
                string password, string private_key)
            {
                if (output_folder[output_folder.Length - 1] != '\\')
                {
                    output_folder += "\\";
                }
                string all_data = File.ReadAllText(encrypted_file_path,
                    Encoding.Unicode);
                string[] all_paths_and_content = all_data.Split(paths_to_content_separator);
                string all_paths = all_paths_and_content[0];
                string all_content = all_paths_and_content[1];
                CreateAllFoldersAndFiles(output_folder, all_paths,
                    password, private_key, all_content);
            }
            private void CreateAllFoldersAndFiles(string output_folder,
                string all_paths, string password, string private_key,
                string all_content)
            {
                Regex folder_regex = new Regex(path_type_separator);
                string[]? folder_paths = null;
                string[]? files_paths = null;
                if (folder_regex.IsMatch(all_paths))
                {
                    string[] folder_files_paths = all_paths.Split(path_type_separator);
                    folder_paths = folder_files_paths[0].Split(folder_path_separator);
                    files_paths = folder_files_paths[1].Split(file_path_separator);
                }
                else
                {
                    files_paths = all_paths.Split(file_path_separator);
                }
                if (folder_paths != null)
                {
                    foreach (string folder_path in folder_paths)
                    {
                        string folder_path_decrypted =
                            DecryptString(folder_path, password, private_key);
                        if (folder_path_decrypted[0] == '\\')
                        {
                            folder_path_decrypted =
                                folder_path_decrypted.Remove(0);
                        }
                        Directory.CreateDirectory(output_folder +
                            folder_path_decrypted);
                    }
                }
                CreateAllFiles(output_folder, files_paths,
                    password, private_key, all_content);
            }
            private void CreateAllFiles(string output_folder,
                string[] files_paths_encrypted,
                string password, string private_key,
                string all_content)
            {
                string[] content_separated =
                    all_content.Split(file_content_separator);
                for (int files_paths_index = 0;
                    files_paths_index < files_paths_encrypted.Length;
                    ++files_paths_index)
                {
                    string encrypted_path = files_paths_encrypted[files_paths_index];
                    string decrypted_path =
                        DecryptString(encrypted_path, password, private_key);
                    string decrypted_file =
                        DecryptString(content_separated[files_paths_index],
                        password, private_key);
                    if (decrypted_path[0] == '\\')
                    {
                        decrypted_path.Remove(0);
                    }
                    byte[] file_bytes = new byte[decrypted_file.Length];
                    for (int char_index = 0;
                        char_index < decrypted_file.Length;
                        ++char_index)
                    {
                        file_bytes[char_index] =
                            (byte)decrypted_file[char_index];
                    }
                    File.WriteAllBytes(output_folder + decrypted_path,
                        file_bytes);
                }
            }
            private void GetAllPaths(string level_0_path,
                string? inner_directory_path,
                ref List<string> all_directories_paths,
                ref List<FilePath> all_files_paths)
            {
                string get_directories = string.Empty;
                if (inner_directory_path == null)
                {
                    get_directories = level_0_path;
                }
                else
                {
                    get_directories = inner_directory_path;
                }
                string[] files = Directory.GetFiles(get_directories);
                foreach (string file_path in files)
                {
                    string relative_file_path =
                        file_path.Replace(level_0_path, "");
                    FilePath filePath = new FilePath();
                    filePath.full_path = file_path;
                    filePath.relative_path = relative_file_path;
                    all_files_paths.Add(filePath);
                }
                string[] directories = Directory.GetDirectories(get_directories);
                foreach (string directory_path in directories)
                {
                    string relative_directory_path =
                        directory_path.Replace(level_0_path, "");
                    all_directories_paths.Add(relative_directory_path);
                    GetAllPaths(level_0_path,
                        directory_path,
                        ref all_directories_paths,
                        ref all_files_paths);
                }
            }
            private string BuildPaths(List<string> all_directories_paths,
                List<FilePath> all_files_paths,
                string password, string private_key)
            {
                string all_paths = string.Empty;
                for (int directory_path_index = 0;
                    directory_path_index < all_directories_paths.Count;
                    ++directory_path_index)
                {
                    string current_directory_path =
                        all_directories_paths[directory_path_index];
                    string encrypted_directory_path =
                        EncryptString(current_directory_path,
                        password, private_key);
                    all_paths += encrypted_directory_path;
                    if (directory_path_index < (all_directories_paths.Count - 1))
                    {
                        all_paths += folder_path_separator;
                    }
                }
                if (all_paths.Length > 0)
                {
                    all_paths += path_type_separator;
                }
                for (int file_path_index = 0;
                    file_path_index < all_files_paths.Count;
                    ++file_path_index)
                {
                    FilePath current_file_path =
                        all_files_paths[file_path_index];
                    string encrypted_file_path =
                        EncryptString(current_file_path.relative_path,
                        password, private_key);
                    all_paths += encrypted_file_path;
                    if (file_path_index < (all_files_paths.Count - 1))
                    {
                        all_paths += file_path_separator;
                    }
                }
                return all_paths;
            }
            public string GeneratePrivateKey()
            {
                return GenerateRandom64String();
            }
            public string GeneratePassword()
            {
                return GenerateRandom64String();
            }
            const int password_pk_length = 64;
            private string GenerateRandom64String()
            {
                string[] chars = new string[]
                {
                    "a", "b", "c", "d", "e", "f", "g", "h", "i",
                    "j", "k", "l", "m", "n", "o", "p", "r", "s",
                    "t", "u", "v", "w", "x", "y", "z",
                    "A", "B", "C", "D", "E", "F", "G", "H", "I",
                    "J", "K", "L", "M", "N", "O", "P", "Q", "R",
                    "S", "T", "U", "V", "W", "X", "Y", "Z",
                    "!", "@", "#", "$", "%", "&", "*",
                    "0", "1", "2", "3", "4", "5", "6", "7", "8",
                    "9"
                };
                string random_string = string.Empty;
                for (int length_index = 0;
                    length_index < password_pk_length;
                    ++length_index)
                {
                    int random_char_index = RandomNew.GetInt(0, chars.Length - 1);
                    random_string += chars[random_char_index];
                }
                return random_string;
            }
            private string EncryptString(string input, string password,
                string private_key)
            {
                string encrypted_input = string.Empty;
                int password_index = 0;
                for (int input_index = 0;
                    input_index < input.Length;
                    ++input_index)
                {
                    int current_int = (int)input[input_index];
                    int password_int = (int)password[password_index];
                    int pk_int = (int)private_key[password_index];
                    ++password_index;
                    if (password_index > (password.Length - 1))
                    {
                        password_index = 0;
                    }
                    int addition = password_int + pk_int;
                    int encrypted_int = current_int + addition;
                    encrypted_input += encrypted_int;
                    if (input_index < (input.Length - 1))
                    {
                        encrypted_input += byte_int_separator;
                    }
                }
                return encrypted_input;
            }
            private string DecryptString(string input, string password,
                string private_key)
            {
                string[] input_ints = input.Split(byte_int_separator);
                int password_index = 0;
                string decrypted_input = string.Empty;
                for (int input_ints_index = 0;
                    input_ints_index < input_ints.Length;
                    ++input_ints_index)
                {
                    int encrypted_int =
                        int.Parse(input_ints[input_ints_index]);
                    int password_int = password[password_index];
                    int pk_int = private_key[password_index];
                    ++password_index;
                    if (password_index > (password_pk_length - 1))
                    {
                        password_index = 0;
                    }
                    int subtraction = password_int + pk_int;
                    int decrypted_int = encrypted_int - subtraction;
                    decrypted_input += (char)decrypted_int;
                }
                return decrypted_input;
            }
        }
        public static void Main(string[] args)
        {
            //SearchFolder("C:\\Users\\ramir\\OneDrive\\Desktop\\3777-main");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}