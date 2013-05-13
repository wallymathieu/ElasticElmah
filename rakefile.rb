
require 'albacore'

task :default => ['elasticelmah:build']
namespace :elasticelmah do
  def nunit_cmd()
    return Dir.glob(File.join(File.dirname(__FILE__),"src","packages","NUnit.Runners.*","tools","nunit-console.exe")).first
  end
  dir = File.dirname(__FILE__)
  desc "build using msbuild"
  msbuild :build do |msb|
    msb.properties :configuration => :Debug
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution =File.join(dir,"src", "ElasticElmah.sln")
  end
  desc "test using nunit console"
  nunit :test => :build do |nunit|
    nunit.command = nunit_cmd()
    nunit.assemblies File.join(dir,"src","ElasticElmah.Appender.Tests/bin/Debug/ElasticElmah.Appender.Tests.dll")
  end
  task :core_copy_to_nuspec => [:build] do
    output_directory_lib = File.join(dir,"nuget/ElasticElmah.Appender/lib/40/")
    mkdir_p output_directory_lib
    cp Dir.glob("./src/ElasticElmah.Appender/bin/Debug/ElasticElmah.Appender.dll"), output_directory_lib
    
  end

  desc "create the nuget package"
  task :nugetpack => [:core_nugetpack]

  task :core_nugetpack => [:core_copy_to_nuspec] do |nuget|
    cd File.join(dir,"nuget/ElasticElmah.Appender") do
      sh "..\\..\\src\\.nuget\\NuGet.exe pack ElasticElmah.Appender.nuspec"
    end
  end


end