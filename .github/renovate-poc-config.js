module.exports = {
  platform: 'github',
  repositories: ['MartinHock/CleanArchitecture'],
  onboarding: false,
  requireConfig: 'ignored',
  baseBranchPatterns: ['poc/renovate-grouped-updates'],
  enabledManagers: ['nuget'],
  dependencyDashboard: false,
  prConcurrentLimit: 10,
  packageRules: [
    {
      description: 'Group non-major NuGet updates into one pull request',
      matchManagers: ['nuget'],
      matchUpdateTypes: ['minor', 'patch', 'pin'],
      groupName: 'NuGet non-major updates',
      groupSlug: 'nuget-non-major'
    },
    {
      description: 'Keep major NuGet updates under manual control',
      matchManagers: ['nuget'],
      matchUpdateTypes: ['major'],
      dependencyDashboardApproval: true
    }
  ]
};
