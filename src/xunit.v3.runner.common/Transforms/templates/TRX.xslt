<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">

  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="assemblies">
    <TestRun>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@user"/>
        <xsl:text>@</xsl:text>
        <xsl:value-of select="@computer"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@timestamp"/>
      </xsl:attribute>
      <xsl:attribute name="runUser">
        <xsl:value-of select="@user"/>
      </xsl:attribute>

      <Times>
        <xsl:attribute name="creation">
          <xsl:value-of select="@start-rtf"/>
        </xsl:attribute>
        <xsl:attribute name="start">
          <xsl:value-of select="@start-rtf"/>
        </xsl:attribute>
        <xsl:attribute name="finish">
          <xsl:value-of select="@finish-rtf"/>
        </xsl:attribute>
      </Times>

      <xsl:if test="assembly/collection/test">
        <Results>
          <xsl:for-each select="assembly/collection/test">
            <UnitTestResult testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d">
              <xsl:attribute name="testName">
                <xsl:value-of select="@name"/>
              </xsl:attribute>
              <xsl:attribute name="testId">
                <xsl:value-of select="@id"/>
              </xsl:attribute>
              <xsl:attribute name="executionId">
                <xsl:value-of select="@id"/>
              </xsl:attribute>
              <xsl:attribute name="computerName">
                <xsl:value-of select="../../../@computer"/>
              </xsl:attribute>
              <xsl:attribute name="outcome">
                <xsl:if test="@result = 'Pass'">Passed</xsl:if>
                <xsl:if test="@result = 'Fail'">Failed</xsl:if>
                <xsl:if test="@result = 'Skip'">NotExecuted</xsl:if>
                <xsl:if test="@result = 'NotRun'">NotExecuted</xsl:if>
              </xsl:attribute>
              <xsl:attribute name="duration">
                <xsl:value-of select="@time-rtf"/>
              </xsl:attribute>
              <xsl:if test="output or failure/message or failure/stack-trace or reason">
                <Output>
                  <xsl:if test="output">
                    <TextMessages>
                      <xsl:call-template name="splitTextMessages">
                        <xsl:with-param name="textMessages" select="output"/>
                      </xsl:call-template>
                    </TextMessages>
                  </xsl:if>
                  <xsl:if test="failure/message or failure/stack-trace">
                    <ErrorInfo>
                      <xsl:if test="failue/message">
                        <Message>
                          <xsl:value-of select="failure/message"/>
                        </Message>
                      </xsl:if>
                      <xsl:if test="failure/stack-trace">
                        <StackTrace>
                          <xsl:value-of select="failure/stack-trace"/>
                        </StackTrace>
                      </xsl:if>
                    </ErrorInfo>
                  </xsl:if>
                  <xsl:if test="reason">
                    <StdOut>
                      <xsl:value-of select="reason"/>
                    </StdOut>
                  </xsl:if>
                </Output>
              </xsl:if>
            </UnitTestResult>
          </xsl:for-each>
        </Results>

        <TestDefinitions>
          <xsl:for-each select="assembly/collection/test">
            <UnitTest>
              <xsl:attribute name="name">
                <xsl:value-of select="@name"/>
              </xsl:attribute>
              <xsl:attribute name="storage">
                <xsl:value-of select="../../@name"/>
              </xsl:attribute>
              <xsl:attribute name="id">
                <xsl:value-of select="@id"/>
              </xsl:attribute>
              <Execution>
                <xsl:attribute name="id">
                  <xsl:value-of select="@id"/>
                </xsl:attribute>
              </Execution>
              <TestMethod>
                <xsl:attribute name="codeBase">
                  <xsl:value-of select="../../@name"/>
                </xsl:attribute>
                <xsl:attribute name="className">
                  <xsl:value-of select="@type"/>
                </xsl:attribute>
                <xsl:attribute name="name">
                  <xsl:value-of select="@method"/>
                </xsl:attribute>
              </TestMethod>
            </UnitTest>
          </xsl:for-each>
        </TestDefinitions>

        <TestEntries>
          <xsl:for-each select="assembly/collection/test">
            <TestEntry testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d">
              <xsl:attribute name="testId">
                <xsl:value-of select="@id"/>
              </xsl:attribute>
              <xsl:attribute name="executionId">
                <xsl:value-of select="@id"/>
              </xsl:attribute>
            </TestEntry>
          </xsl:for-each>
        </TestEntries>

        <TestLists>
          <TestList name="Results Not in a List" id="8c84fa94-04c1-424b-9868-57a2d4851a1d" />
          <TestList name="All Loaded Results" id="19431567-8539-422a-85d7-44ee4e166bda" />
        </TestLists>
      </xsl:if>

      <ResultSummary>
        <xsl:attribute name="outcome">
          <xsl:if test="sum(assembly/@failed) > 0">Failed</xsl:if>
          <xsl:if test="sum(assembly/@failed) = 0">Passed</xsl:if>
        </xsl:attribute>
        <Counters>
          <xsl:attribute name="total">
            <xsl:value-of select="sum(assembly/@total)"/>
          </xsl:attribute>
          <xsl:attribute name="executed">
            <xsl:value-of select="sum(assembly/@total) - sum(assembly/@failed) - sum(assembly/@skipped) - sum(assembly/@not-run)"/>
          </xsl:attribute>
          <xsl:attribute name="failed">
            <xsl:value-of select="sum(assembly/@failed)"/>
          </xsl:attribute>
          <xsl:attribute name="notExecuted">
            <xsl:value-of select="sum(assembly/@skipped) + sum(assembly/@not-run)"/>
          </xsl:attribute>
        </Counters>
      </ResultSummary>
    </TestRun>
  </xsl:template>

  <xsl:template name="splitTextMessages">
    <xsl:param name="textMessages" select="."/>
    <xsl:variable name="textMessage">
      <xsl:value-of select="normalize-space(substring-before($textMessages, '&#xD;&#xA;'))"/>
    </xsl:variable>
    <xsl:if test="string-length($textMessages)">
      <xsl:if test="normalize-space($textMessages) != ''">
        <Message>
          <xsl:value-of select="$textMessage"/>
        </Message>
      </xsl:if>
      <xsl:call-template name="splitTextMessages">
        <xsl:with-param name="textMessages" select="substring-after($textMessages, '&#xD;&#xA;')"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
