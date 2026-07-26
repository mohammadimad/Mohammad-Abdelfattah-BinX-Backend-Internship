README

# 📂 Day 01: Onboarding, .NET SDK Setup & IDE Configuration

## 📝 Objective

The primary focus of Day 1 was to establish a professional, enterprise-grade development environment, understand the 10-week internship roadmap, and verify the runtime and compilation tools.

---

## 🛠️ Completed Tasks

1. **Environment Installation:**
   - Installed and verified the **.NET 8.0 SDK (LTS)** on the local machine.
   - Configured system environment PATH variables.

2. **Tooling Verification via CLI:**
   - Checked active SDK version using:
     ```bash
     dotnet --version
     ```
   - Listed all installed SDK runtimes using:
     ```bash
     dotnet --list-sdks
     ```

3. **IDE Integration:**
   - Configured **Visual Studio 2022** with active compiler tools and validated the debugging workspace.

4. **Console Application Initialization:**
   - Scaffolded the initial `HelloBinX` console application.
   - Implemented custom outputs printing user developer details and current system timestamp.
   - Verified local compilation and execution via CLI:
     ```bash
     dotnet run
     ```

5. **Workspace Git Configuration:**
   - Configured a professional `.gitignore` to protect the repository from committing heavy IDE-specific binaries and local caches (`.vs/`, `bin/`, `obj/`).
